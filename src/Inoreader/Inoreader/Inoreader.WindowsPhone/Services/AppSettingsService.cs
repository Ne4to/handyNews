using System;
using Windows.Storage;

namespace Inoreader.Services
{
	public class AppSettingsService
	{
		private const string SettingsContainerName = "AppSettings";
		private readonly ApplicationDataContainer _rootContainer = ApplicationData.Current.LocalSettings;

		public string DisplayCulture { get; set; }
		public bool HideEmptySubscriptions { get; set; }
		public bool ShowNewestFirst { get; set; }

		public AppSettingsService()
		{
			DisplayCulture = String.Empty;
			HideEmptySubscriptions = true;
			ShowNewestFirst = true;

			Load();
		}

		private void Load()
		{
			ApplicationDataContainer container;
			if (!_rootContainer.Containers.TryGetValue(SettingsContainerName, out container))
				return;

			DisplayCulture = container.GetValue("DisplayCulture", String.Empty);
			HideEmptySubscriptions = container.GetValue("HideEmptySubscriptions", true);
			ShowNewestFirst = container.GetValue("ShowNewestFirst", true);
		}

		public void Save()
		{
			var container = _rootContainer.CreateContainer(SettingsContainerName, ApplicationDataCreateDisposition.Always);
			container.Values["DisplayCulture"] = DisplayCulture;
			container.Values["HideEmptySubscriptions"] = HideEmptySubscriptions;
			container.Values["ShowNewestFirst"] = ShowNewestFirst;
		}
	}
}