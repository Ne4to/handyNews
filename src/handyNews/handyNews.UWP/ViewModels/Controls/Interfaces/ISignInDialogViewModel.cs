using System.ComponentModel;
using System.Threading.Tasks;

namespace handyNews.UWP.ViewModels.Controls.Interfaces
{
    public interface ISignInDialogViewModel : INotifyPropertyChanged
    {
        string Email { get; set; }
        string Password { get; set; }
        bool IsBusy { get; set; }
        string SignInError { get; set; }
        bool SignInAvailable { get; }
        Task<bool> SignInAsync();
#if DEBUG
        void SetDebugUser();
#endif
    }
}