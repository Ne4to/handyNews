using Windows.Security.Credentials;

namespace Inoreader.Domain.Services
{
	public interface ICredentialService
	{
		void Save(string userName, string password);
		PasswordCredential Load();
	}
}