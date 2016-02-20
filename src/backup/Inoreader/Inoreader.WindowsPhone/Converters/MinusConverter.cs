using System;
using Windows.UI.Xaml.Data;

namespace Inoreader.Converters
{
	public class MinusConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var res = -(double)value;
			return res;			
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}