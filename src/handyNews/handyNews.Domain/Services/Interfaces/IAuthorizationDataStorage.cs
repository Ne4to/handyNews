using System;

namespace handyNews.Domain.Services.Interfaces
{
    public interface IAuthorizationDataStorage
    {
        string RefreshToken { get; set; }
        DateTimeOffset? AccessTokenExpireDate { get; set; }
        string AccessToken { get; set; }

        void Clear();
        void Save();
    }
}