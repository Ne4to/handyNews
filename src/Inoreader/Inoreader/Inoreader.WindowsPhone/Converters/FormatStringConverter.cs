using System;
using Windows.UI.Xaml.Data;

namespace Inoreader.Converters
{
	public class FormatStringConverter : IValueConverter
	{
		public string StringFormat { get; set; }

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var formattable = value as IFormattable;
			if (formattable != null)
			{
				return formattable.ToString(StringFormat, null);
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}