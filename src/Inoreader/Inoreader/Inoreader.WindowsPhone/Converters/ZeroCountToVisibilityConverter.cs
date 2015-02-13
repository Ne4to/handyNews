using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Inoreader.Converters
{
	public class ZeroCountToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			// only one empty item
			return (int) value == 1 ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}