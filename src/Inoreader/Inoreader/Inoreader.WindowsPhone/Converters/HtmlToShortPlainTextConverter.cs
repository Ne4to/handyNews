using System;
using Windows.UI.Xaml.Data;
using Inoreader.Services;

namespace Inoreader.Converters
{
	public class HtmlToShortPlainTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var str = (string) value;
			return HtmlParser.GetPlainText(str, 200);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}