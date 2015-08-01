using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Inoreader.Api;
using Inoreader.Api.Exceptions;
using Inoreader.Domain.Services.Interfaces;

namespace Inoreader.Domain.Services
{
    public class SignInManager : ISignInManager
    {
        private readonly ApiClient _apiClient;
        private readonly ISessionStore _sessionStore;

        public SignInManager(ApiClient apiClient, ISessionStore sessionStore)
        {
            if (apiClient == null) throw new ArgumentNullException(nameof(apiClient));
            if (sessionStore == null) throw new ArgumentNullException(nameof(sessionStore));

            _apiClient = apiClient;
            _sessionStore = sessionStore;
        }

        public bool SignInRequired => string.IsNullOrEmpty(_sessionStore.Auth);

        public async Task SignInAsync(string email, string password)
        {
            var result = await _apiClient.SignInAsync(email, password).ConfigureAwait(false);

            if (!result.Success)
            {
                throw new AuthenticationApiException(result.ErrorMessage);
            }

            _sessionStore.SID = result.SID;
            _sessionStore.LSID = result.LSID;
            _sessionStore.Auth = result.Auth;
            _sessionStore.Save();            
        }

        public void SignOut()
        {
            _sessionStore.Clear();
            _sessionStore.Save();
        }
    }
}