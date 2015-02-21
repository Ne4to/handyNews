using System;
using System.Collections.Generic;
using Windows.Storage;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Inoreader.Services
{
	public static class Extensions
	{
		public static T GetValue<T>(this ApplicationDataContainer container, string key, T defaultValue)
		{
			object obj;
			if (container.Values.TryGetValue(key, out obj))
				return (T)obj;

			return defaultValue;
		}

		public static T GetValue<T>(this IDictionary<string, object> dictionary, string key, T defaultValue = default (T))
		{
			object o;
			if (!dictionary.TryGetValue(key, out o))
				return defaultValue;

			return (T)o;
		}

		public static void TrackExceptionFull(this TelemetryClient telemetryClient, Exception exception)
		{
			var exceptionTelemetry = new ExceptionTelemetry(exception);
			exceptionTelemetry.Properties.Add("Ex.StackTrace", exception.StackTrace);
			exceptionTelemetry.Properties.Add("Ex.Source", exception.Source);
			exceptionTelemetry.Properties.Add("Ex.Message", exception.Message);
			
			var baseException = exception.GetBaseException();
			if (baseException != null)
			{
				exceptionTelemetry.Properties.Add("Ex.Base.Message", baseException.Message);
				exceptionTelemetry.Properties.Add("Ex.Base.Source", baseException.Source);
				exceptionTelemetry.Properties.Add("Ex.Base.StackTrace", baseException.StackTrace);
			}

			telemetryClient.TrackException(exceptionTelemetry);
		}

		public static DateTime GetBeginWeekDate(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
		{
			int diff = date.DayOfWeek - startOfWeek;
			if (diff < 0)
			{
				diff += 7;
			}

			return date.AddDays(-1 * diff).Date;
		}
	}
}