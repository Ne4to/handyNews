using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using handyNews.Domain.Services.Interfaces;
using JetBrains.Annotations;

namespace handyNews.Domain.Services
{
    public class AuthorizationHandler : DelegatingHandler
    {
        private readonly IAuthenticationManager _authenticationManager;
        private readonly IAuthorizationDataStorage _authorizationDataStorage;

        public AuthorizationHandler([NotNull] IAuthorizationDataStorage authorizationDataStorage,
            [NotNull] IAuthenticationManager authenticationManager)
        {
            if (authorizationDataStorage == null) throw new ArgumentNullException(nameof(authorizationDataStorage));
            if (authenticationManager == null) throw new ArgumentNullException(nameof(authenticationManager));
            _authorizationDataStorage = authorizationDataStorage;
            _authenticationManager = authenticationManager;
        }

        protected AuthorizationHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_authorizationDataStorage.AccessToken == null)
                throw new Exception("TODO user is not authenticated");

            if (_authorizationDataStorage.AccessTokenExpireDate <= DateTimeOffset.UtcNow)
            {
                await _authenticationManager.RefreshTokenAsync().ConfigureAwait(false);
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
                _authorizationDataStorage.AccessToken);

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}