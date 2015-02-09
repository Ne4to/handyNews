using Windows.Storage;

namespace Inoreader.Services
{
	public static class ApplicationDataContainerExtensions
	{
		public static T GetValue<T>(this ApplicationDataContainer container, string key, T defaultValue)
		{
			object obj;
			if (container.Values.TryGetValue(key, out obj))
				return (T)obj;

			return defaultValue;
		}
	}
}