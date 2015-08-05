using System;
using System.Collections.Generic;
using Windows.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inoreader.Domain.Utils
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

		public static DateTime GetBeginWeekDate(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
		{
			int diff = date.DayOfWeek - startOfWeek;
			if (diff < 0)
			{
				diff += 7;
			}

			return date.AddDays(-1 * diff).Date;
		}

		public static string ToJson(this object obj)
		{
			return JObject.FromObject(obj).ToString(Formatting.None);
		}

		public static T FromJson<T>(this string jsonString, bool supressErrors = false)
		{
			try
			{
				return JObject.Parse(jsonString).ToObject<T>();
			}
			catch (Exception)
			{
				if (supressErrors)
					return default(T);

				throw;
			}
		}
	}
}