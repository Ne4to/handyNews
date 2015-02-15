using System;
using Windows.UI.Xaml.Data;

namespace Inoreader.Converters
{
	public class FileSizeToStringConverter : IValueConverter
	{
		private const ulong KB = 1024;
		private const ulong MB = KB * 1024;
		private const ulong GB = MB * 1024;

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var size = (ulong)value;

			if (size > GB)
				return String.Format("{0:F1} GB", (double)size / GB);

			if (size > MB)
				return String.Format("{0:F1} MB", (double)size / MB);

			if (size > KB)
				return String.Format("{0:F1} KB", (double)size / KB);

			return String.Format("{0} {1}", size, Strings.Resources.FileSizeBytes);

		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}