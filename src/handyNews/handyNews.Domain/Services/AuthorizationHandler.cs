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
        private readonly IAuthorizationDataStorage _authorizationDataStorage;

        public AuthorizationHandler([NotNull] IAuthorizationDataStorage authorizationDataStorage)
        {
            if (authorizationDataStorage == null) throw new ArgumentNullException(nameof(authorizationDataStorage));
            _authorizationDataStorage = authorizationDataStorage;
        }

        protected AuthorizationHandler(HttpMessageHandler innerHandler) 
            : base(innerHandler)
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_authorizationDataStorage.AccessToken == null)
                throw new Exception("TODO user is not authenticated");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authorizationDataStorage.AccessToken);

            return base.SendAsync(request, cancellationToken);
        }
    }
}