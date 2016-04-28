using System.Threading.Tasks;
using handyNews.API.Exceptions;

namespace handyNews.Domain.Services.Interfaces
{
    public interface IAuthenticationManager
    {
        bool IsUserAuthenticated { get; }

        /// <exception cref="AuthenticationApiException"></exception>		
        Task<bool> SignInAsync();

        void SignOut();
    }
}