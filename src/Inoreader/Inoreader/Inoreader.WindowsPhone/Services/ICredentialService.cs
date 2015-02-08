using Windows.Security.Credentials;

namespace Inoreader.Services
{
	public interface ICredentialService
	{
		void Save(string userName, string password);
		PasswordCredential Load();
	}
}