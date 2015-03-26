using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Data;
using Inoreader.Services;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.ServiceLocation;

namespace Inoreader.Converters
{
	public class HtmlToShortPlainTextConverter : IValueConverter
	{
		private readonly TelemetryClient _telemetryClient;

		public HtmlToShortPlainTextConverter()
		{
			if (!DesignMode.DesignModeEnabled)
			{
				_telemetryClient = ServiceLocator.Current.GetInstance<TelemetryClient>();
			}
		}

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			try
			{
				var str = (string) value;
				var parser = new HtmlParser();
				return parser.GetPlainText(str, 200);
			}
			catch (Exception e)
			{
				_telemetryClient.TrackException(e);
				return String.Empty;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}