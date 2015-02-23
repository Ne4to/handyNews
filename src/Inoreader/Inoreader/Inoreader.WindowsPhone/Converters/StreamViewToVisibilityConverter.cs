using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Inoreader.Services;

namespace Inoreader.Converters
{
	public class StreamViewToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, System.Type targetType, object parameter, string language)
		{
			bool equals = (StreamView) value == (StreamView) parameter;
			return equals ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, System.Type targetType, object parameter, string language)
		{
			throw new System.NotImplementedException();
		}
	}
}