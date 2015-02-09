using System;
using Windows.Storage;

namespace Inoreader.Services
{
	public class AppSettingsService
	{
		private const string SettingsContainerName = "AppSettings";
		private readonly ApplicationDataContainer _rootContainer = ApplicationData.Current.LocalSettings;

		public string DisplayCulture { get; set; }

		public AppSettingsService()
		{
			DisplayCulture = String.Empty;

			Load();
		}

		private void Load()
		{
			ApplicationDataContainer container;
			if (!_rootContainer.Containers.TryGetValue(SettingsContainerName, out container))
				return;

			DisplayCulture = container.GetValue("DisplayCulture", String.Empty);
		}

		public void Save()
		{
			var container = _rootContainer.CreateContainer(SettingsContainerName, ApplicationDataCreateDisposition.Always);
			container.Values["DisplayCulture"] = DisplayCulture;
		}
	}
}