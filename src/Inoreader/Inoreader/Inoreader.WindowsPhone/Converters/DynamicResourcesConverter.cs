using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Data;
using Inoreader.Services;
using Microsoft.Practices.ServiceLocation;

namespace Inoreader.Converters
{
	public class DynamicResourcesConverter : IValueConverter
	{
		private readonly AppSettingsService _appSettings;

		public DynamicResourcesConverter()
		{
			if (DesignMode.DesignModeEnabled)
				return;

			_appSettings = ServiceLocator.Current.GetInstance<AppSettingsService>();
		}

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (_appSettings == null)
				return null;

			switch (parameter.ToString())
			{
				case "StreamTitleFontSize":
					return _appSettings.StreamTitleFontSize;

				case "StreamDateFontSize":
					return _appSettings.StreamDateFontSize;

				case "PageHeaderFontSize":
					return _appSettings.PageHeaderFontSize;

				case "SubscriptionTreeItemFontSize":
					return _appSettings.SubscriptionTreeItemFontSize;

				default:
					throw new ArgumentOutOfRangeException("parameter");
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
