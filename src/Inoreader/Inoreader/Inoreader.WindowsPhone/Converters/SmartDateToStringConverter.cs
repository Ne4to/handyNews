using System;
using Windows.UI.Xaml.Data;
using Inoreader.Services;

namespace Inoreader.Converters
{
	public class SmartDateToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null)
				return null;

			var localDate = ((DateTimeOffset)value).LocalDateTime;

			var today = DateTime.Today;

			// Short time pattern.
			var isToday = localDate.Date == today;
			if (isToday)
				return localDate.ToString("t");

			// The abbreviated name of the day of the week.
			var thisWeek = localDate >= today.GetBeginWeekDate();
			if (thisWeek)
				return localDate.ToString("ddd");

			// Month/day pattern.
			var thisYear = localDate.Year == today.Year;
			if (thisYear)
				return localDate.ToString("m");

			// Short date pattern.
			return localDate.ToString("d"); 
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}