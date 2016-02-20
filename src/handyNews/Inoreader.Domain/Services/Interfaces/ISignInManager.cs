using System.Threading.Tasks;

namespace handyNews.Domain.Services.Interfaces
{
    public interface ISignInManager
    {
        bool SignInRequired { get; }

        /// <exception cref="AuthenticationApiException"></exception>		
        Task SignInAsync(string email, string password);

        void SignOut();
    }
}