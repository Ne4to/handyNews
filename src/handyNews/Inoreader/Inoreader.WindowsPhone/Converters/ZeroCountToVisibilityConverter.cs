using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Inoreader.Converters
{
	public class ZeroCountToVisibilityConverter : IValueConverter
	{
		public int VisibleCount { get; set; }

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			// only one empty item
			return (int)value == VisibleCount ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}