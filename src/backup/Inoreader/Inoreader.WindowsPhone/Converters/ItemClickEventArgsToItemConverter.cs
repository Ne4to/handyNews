using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Inoreader.Converters
{
	public class ItemClickEventArgsToItemConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return ((ItemClickEventArgs) value).ClickedItem;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}