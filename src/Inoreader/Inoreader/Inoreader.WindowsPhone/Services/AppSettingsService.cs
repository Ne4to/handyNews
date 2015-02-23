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
		public StreamView StreamView { get; set; }
		public double FontSize { get; set; }

		public AppSettingsService()
		{
			DisplayCulture = String.Empty;
			HideEmptySubscriptions = true;
			ShowNewestFirst = true;
			StreamView = StreamView.ExpandedView;
			FontSize = 11D;

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
			StreamView = (StreamView)container.GetValue("StreamView", (int)StreamView.ExpandedView);
			FontSize = container.GetValue("FontSize", 11D);
		}

		public void Save()
		{
			var container = _rootContainer.CreateContainer(SettingsContainerName, ApplicationDataCreateDisposition.Always);
			container.Values["DisplayCulture"] = DisplayCulture;
			container.Values["HideEmptySubscriptions"] = HideEmptySubscriptions;
			container.Values["ShowNewestFirst"] = ShowNewestFirst;
			container.Values["StreamView"] = (int)StreamView;
			container.Values["FontSize"] = FontSize;
		}
	}

	public enum StreamView
	{
		ExpandedView = 0,
		ListView = 1
	}
}