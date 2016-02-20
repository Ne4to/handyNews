using System;
using Windows.UI.Xaml.Data;

namespace Inoreader.Converters
{
	public class DebugConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return value;
		}
	}
}