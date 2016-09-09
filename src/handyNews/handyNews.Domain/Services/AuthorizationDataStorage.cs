using System;
using Windows.Storage;
using handyNews.Domain.Services.Interfaces;

namespace handyNews.Domain.Services
{
    public class AuthorizationDataStorage : IAuthorizationDataStorage
    {
        private readonly ApplicationDataContainer _rootContainer = ApplicationData.Current.RoamingSettings;
        private const string SETTINGS_CONTAINER_NAME = "handyNews.AccessToken";

        public string AccessToken { get; set; }
        public DateTimeOffset? AccessTokenExpireDate { get; set; }
        public string RefreshToken { get; set; }

        public AuthorizationDataStorage()
        {
            Load();
        }

        public void Save()
        {
            var container = _rootContainer.CreateContainer(SETTINGS_CONTAINER_NAME,
                ApplicationDataCreateDisposition.Always);
            container.Values[nameof(AccessToken)] = AccessToken;
            container.Values[nameof(AccessTokenExpireDate)] = AccessTokenExpireDate;
            container.Values[nameof(RefreshToken)] = RefreshToken;
        }

        public void Clear()
        {
            AccessToken = null;
            AccessTokenExpireDate = null;
            RefreshToken = null;
        }

        private void Load()
        {
            ApplicationDataContainer container;
            if (!_rootContainer.Containers.TryGetValue(SETTINGS_CONTAINER_NAME, out container))
            {
                return;
            }

            AccessToken = GetValue(container, nameof(AccessToken), default(string));
            AccessTokenExpireDate = GetValue(container, nameof(AccessTokenExpireDate), default(DateTimeOffset?));
            RefreshToken = GetValue(container, nameof(RefreshToken), default(string));
        }

        private T GetValue<T>(ApplicationDataContainer container, string key, T defaultValue)
        {
            object obj;
            if (container.Values.TryGetValue(key, out obj))
            {
                return (T) obj;
            }

            return defaultValue;
        }
    }
}