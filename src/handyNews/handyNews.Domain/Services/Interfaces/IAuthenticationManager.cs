using System.Threading.Tasks;

namespace handyNews.Domain.Services.Interfaces
{
    public interface IAuthenticationManager
    {
        bool IsUserAuthenticated { get; }

        /// <exception cref="AuthenticationApiException"></exception>
        Task<bool> SignInAsync();

        Task RefreshTokenAsync();
        void SignOut();
    }
}