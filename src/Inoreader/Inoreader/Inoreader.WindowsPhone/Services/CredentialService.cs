using System;
using System.Linq;
using Windows.Security.Credentials;

namespace Inoreader.Services
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class CredentialService : ICredentialService
	{
		private const string AppResourceName = "Inoreader Free";

		public void Save(string userName, string password)
		{
			Clear();

			PasswordVault vault = new PasswordVault();
			vault.Add(new PasswordCredential(AppResourceName, userName, password));
		}

		private void Clear()
		{
			try
			{
				PasswordVault vault = new PasswordVault();
				var items = vault.FindAllByResource(AppResourceName);
				foreach (var item in items)
				{
					vault.Remove(item);
				}
			}
			// ReSharper disable once EmptyGeneralCatchClause
			catch (Exception)
			{
			}
		}

		public PasswordCredential Load()
		{
			try
			{
				PasswordVault vault2 = new PasswordVault();
				return vault2.FindAllByResource(AppResourceName).FirstOrDefault();
			}
			catch (Exception)
			{
				return null;
			}
		}	
	}
}