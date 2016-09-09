using System;

namespace handyNews.Inoreader.Exceptions
{
    public class AuthenticationApiException : ApiException
    {
        public AuthenticationApiException()
        {
        }

        public AuthenticationApiException(string message)
            : base(message)
        {
        }

        public AuthenticationApiException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}