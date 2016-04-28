using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using handyNews.Domain.Services.Interfaces;
using Newtonsoft.Json;

namespace handyNews.Domain.Services
{
    public class AuthenticationManager : IAuthenticationManager
    {
        const string SCOPES = "read write";

        private readonly IAuthorizationDataStorage _authorizationDataStorage;

        public AuthenticationManager(IAuthorizationDataStorage authorizationDataStorage)
        {
            if (authorizationDataStorage == null) throw new ArgumentNullException(nameof(authorizationDataStorage));

            _authorizationDataStorage = authorizationDataStorage;
        }

        public bool IsUserAuthenticated => !string.IsNullOrEmpty(_authorizationDataStorage.AccessToken);

        public async Task<bool> SignInAsync()
        {
            var clientData = await GetClientDataAsync();

            var authorizationCode = await GetAuthorizationCode(clientData).ConfigureAwait(false);
            var accessTokenData = await GetAccessTokenData(authorizationCode, clientData).ConfigureAwait(false);

            _authorizationDataStorage.AccessToken = accessTokenData.AccessToken;
            _authorizationDataStorage.AccessTokenExpireDate = DateTimeOffset.UtcNow.AddSeconds(accessTokenData.ExpiresIn);
            _authorizationDataStorage.RefreshToken = accessTokenData.RefreshToken;
            _authorizationDataStorage.Save();

            //TelemetryManager.TrackEvent(TelemetryEvents.SignIn);

            return true;
        }

        private async Task<string> GetAuthorizationCode(ClientData clientData)
        {
            Uri callbackUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri();
            var state = Guid.NewGuid().ToString("N");
            var uri = $"https://www.inoreader.com/oauth2/auth?client_id={clientData.ClientId}&redirect_uri={Uri.EscapeUriString(callbackUri.ToString())}&response_type=code&scope={Uri.EscapeUriString(SCOPES)}&state={state}";

            var authenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, new Uri(uri)).AsTask().ConfigureAwait(false);
            var authorizationCodeResponseDataUri = new Uri(authenticationResult.ResponseData);
            var authorizationCodeResponseData = authorizationCodeResponseDataUri.GetComponents(UriComponents.Query, UriFormat.Unescaped)
                .Split('&')
                .Select(str => str.Split('='))
                .ToDictionary(arr => arr[0], arr => arr[1]);

            if (authorizationCodeResponseData["state"] != state)
                throw new Exception("Invalid state");

            return authorizationCodeResponseData["code"];
        }

        private async Task<AccessTokenData> GetAccessTokenData(string authorizationCode, ClientData clientData)
        {
            var httpClient = new HttpClient();

            var callbackUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri();
            var httpContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", authorizationCode),
                new KeyValuePair<string, string>("redirect_uri", callbackUri.ToString()),
                new KeyValuePair<string, string>("client_id", clientData.ClientId),
                new KeyValuePair<string, string>("client_secret", clientData.ClientSecret),
                new KeyValuePair<string, string>("scope", string.Empty),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
            });

            var getAccessTokenResponseMessage = await httpClient.PostAsync("https://www.inoreader.com/oauth2/token", httpContent).ConfigureAwait(false);
            var accessTokenDataJson = await getAccessTokenResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<AccessTokenData>(accessTokenDataJson);
        }

        public void SignOut()
        {
            _authorizationDataStorage.Clear();
            _authorizationDataStorage.Save();
        }

        // TODO implement RefreshTokenMethod

        private async Task<ClientData> GetClientDataAsync()
        {
            var fileUri = new Uri("ms-appx:///Assets/AppSecret.json");
            var file = await StorageFile.GetFileFromApplicationUriAsync(fileUri).AsTask().ConfigureAwait(false);
            var fileContent = await FileIO.ReadTextAsync(file).AsTask().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<ClientData>(fileContent);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        class AccessTokenData
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }
            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }
            [JsonProperty("token_type")]
            public string TokenType { get; set; }
            [JsonProperty("scope")]
            public string Scope { get; set; }
            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        class ClientData
        {
            [JsonProperty("clientId")]
            public string ClientId { get; set; }
            [JsonProperty("clientSecret")]
            public string ClientSecret { get; set; }
        }
    }
}