using System;

namespace handyNews.API.Exceptions
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