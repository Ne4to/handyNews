using System;
using Inoreader.Api;
using Inoreader.Domain.Services.Interfaces;

namespace Inoreader.Domain.Services
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