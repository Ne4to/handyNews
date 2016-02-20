using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Inoreader.Converters
{
	public class BooleanToBrushConverter : IValueConverter
	{
		public Brush TrueBrush { get; set; }
		public Brush FalseBrush { get; set; }

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return (bool)value ? TrueBrush : FalseBrush;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}