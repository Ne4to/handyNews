using Windows.Storage;
using handyNews.Domain.Services.Interfaces;

namespace handyNews.Domain.Services 
{
	public class SessionStore : ISessionStore
    {
#if WINDOWS_UWP
        private const string SettingsContainerName = "handyNews.Session";
#else
        private const string SettingsContainerName = "API.Session";
#endif

        private readonly ApplicationDataContainer _rootContainer = ApplicationData.Current.RoamingSettings;

		// ReSharper disable once InconsistentNaming
		public string SID { get; set; }
		// ReSharper disable once InconsistentNaming
		public string LSID { get; set; }
		public string Auth { get; set; }

		public SessionStore()
		{
			Load();
		}

		private void Load()
		{
			ApplicationDataContainer container;
			if (!_rootContainer.Containers.TryGetValue(SettingsContainerName, out container))
				return;

			SID = GetValue(container, "SID", default(string));
			LSID = GetValue(container, "LSID", default(string));
			Auth = GetValue(container, "Auth", default(string));
		}

		private static T GetValue<T>(ApplicationDataContainer container, string key, T defaultValue)
		{
			object obj;
			if (container.Values.TryGetValue(key, out obj))
				return (T)obj;

			return defaultValue;
		}

		public void Save()
		{
			var container = _rootContainer.CreateContainer(SettingsContainerName, ApplicationDataCreateDisposition.Always);
			container.Values["SID"] = SID;
			container.Values["LSID"] = LSID;
			container.Values["Auth"] = Auth;
		}

		public void Clear()
		{
			SID = null;
			LSID = null;
			Auth = null;
		}
	}
}