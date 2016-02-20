using System;
using handyNews.API;
using handyNews.Domain.Services.Interfaces;

namespace handyNews.Domain.Services
{
    public class ApiSession : IApiSession
    {
        private readonly ISessionStore _sessionStore;

        public string AuthenticationToken => _sessionStore.Auth;

        public ApiSession(ISessionStore sessionStore)
        {
            if (sessionStore == null) throw new ArgumentNullException(nameof(sessionStore));
            _sessionStore = sessionStore;
        }
    }
}