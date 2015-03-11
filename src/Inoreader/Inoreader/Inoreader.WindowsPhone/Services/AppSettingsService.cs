﻿using System;
using Windows.Storage;

namespace Inoreader.Services
{
	public class AppSettingsService
	{
		private const double DefaultFontSize = 11D;
		private const double StreamTitleFontSizeMult = 14D / DefaultFontSize;
		private const double StreamDateFontSizeMult = 11D / DefaultFontSize;
		private const double FontSizeH1Mult = 28D / DefaultFontSize;
		private const double FontSizeH2Mult = 24D / DefaultFontSize;
		private const double FontSizeH3Mult = 20D / DefaultFontSize;
		private const double FontSizeH4Mult = 17D / DefaultFontSize;
		private const double FontSizeH5Mult = 14D / DefaultFontSize;
		private const double FontSizeH6Mult = 12D / DefaultFontSize;

		private const double PageHeaderFontSizeMult = 24D / DefaultFontSize;
		private const double SubscriptionTreeItemFontSizeMult = 18D / DefaultFontSize;
		
		private const string SettingsContainerName = "AppSettings";
		private readonly ApplicationDataContainer _rootContainer = ApplicationData.Current.LocalSettings;

		public string DisplayCulture { get; set; }
		public bool HideEmptySubscriptions { get; set; }
		public bool ShowNewestFirst { get; set; }
		public StreamView StreamView { get; set; }
		public double FontSize { get; set; }

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