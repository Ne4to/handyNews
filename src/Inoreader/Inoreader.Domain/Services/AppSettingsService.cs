using System;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Inoreader.Domain.Services
{
	public class AppSettingsService
	{
		private const double DefaultFontSize = 11D;
		private const double StreamTitleFontSizeMult = 14D / DefaultFontSize;
		private const double StreamDateFontSizeMult = 11D / DefaultFontSize;
		private const double FontSizeH1Mult = 2D; // in em, em = 16px
		private const double FontSizeH2Mult = 1.5D; // in em, em = 16px
		private const double FontSizeH3Mult = 1.17D; // in em, em = 16px
		private const double FontSizeH4Mult = 1D; // in em, em = 16px
		private const double FontSizeH5Mult = .83D; // in em, em = 16px
		private const double FontSizeH6Mult = .67D; // in em, em = 16px

		private const double PageHeaderFontSizeMult = 24D / DefaultFontSize;
		private const double SubscriptionTreeItemFontSizeMult = 18D / DefaultFontSize;

		private const string SettingsContainerName = "AppSettings";
		private readonly ApplicationDataContainer _rootContainer = ApplicationData.Current.LocalSettings;

		public string DisplayCulture { get; set; }
		public bool HideEmptySubscriptions { get; set; }
		public bool ShowNewestFirst { get; set; }
		public bool AutoMarkAsRead { get; set; }
		public StreamView StreamView { get; set; }
		public TextAlignment TextAlignment { get; set; }
		public double FontSize { get; set; }
		public int PreloadItemCount { get; set; }
		
		public double StreamTitleFontSize
		{
			get { return FontSize * StreamTitleFontSizeMult; }
		}

		public double StreamDateFontSize
		{
			get { return FontSize * StreamDateFontSizeMult; }
		}

		public double FontSizeH1
		{
			get { return FontSize * FontSizeH1Mult; }
		}

		public double FontSizeH2
		{
			get { return FontSize * FontSizeH2Mult; }
		}

		public double FontSizeH3
		{
			get { return FontSize * FontSizeH3Mult; }
		}

		public double FontSizeH4
		{
			get { return FontSize * FontSizeH4Mult; }
		}

		public double FontSizeH5
		{
			get { return FontSize * FontSizeH5Mult; }
		}

		public double FontSizeH6
		{
			get { return FontSize * FontSizeH6Mult; }
		}

		public double PageHeaderFontSize
		{
			get { return FontSize * PageHeaderFontSizeMult; }
		}

		public double SubscriptionTreeItemFontSize
		{
			get { return FontSize * SubscriptionTreeItemFontSizeMult; }
		}

		public AppSettingsService()
		{
			DisplayCulture = String.Empty;
			HideEmptySubscriptions = true;
			ShowNewestFirst = true;
			StreamView = StreamView.ExpandedView;
			FontSize = 11D;
			TextAlignment = TextAlignment.Justify;
			AutoMarkAsRead = StreamView == StreamView.ExpandedView;
			PreloadItemCount = 10;

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
			TextAlignment = (TextAlignment)container.GetValue("TextAlignment", (int)TextAlignment.Justify);
			PreloadItemCount = container.GetValue("PreloadItemCount", 10);
			
			// This setting did not exist in app version <= 1.1.3.15
			// If user updates app do not change behaviour
			AutoMarkAsRead = StreamView == StreamView.ExpandedView;
			if (container.Values.ContainsKey("AutoMarkAsRead"))
			{
				AutoMarkAsRead = container.GetValue("AutoMarkAsRead", true);
			}
		}

		public void Save()
		{
			var container = _rootContainer.CreateContainer(SettingsContainerName, ApplicationDataCreateDisposition.Always);
			container.Values["DisplayCulture"] = DisplayCulture;
			container.Values["HideEmptySubscriptions"] = HideEmptySubscriptions;
			container.Values["ShowNewestFirst"] = ShowNewestFirst;
			container.Values["StreamView"] = (int)StreamView;
			container.Values["FontSize"] = FontSize;
			container.Values["TextAlignment"] = (int)TextAlignment;
			container.Values["AutoMarkAsRead"] = AutoMarkAsRead;
			container.Values["PreloadItemCount"] = PreloadItemCount;
		}
	}

	public enum StreamView
	{
		ExpandedView = 0,
		ListView = 1
	}
}