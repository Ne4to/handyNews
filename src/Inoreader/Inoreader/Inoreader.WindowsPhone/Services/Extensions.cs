using System.Collections.Generic;
using Windows.Storage;

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
	}
}