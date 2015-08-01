using System;
using System.Threading.Tasks;
using handyNews.UWP.Model;
using Inoreader;
using Inoreader.Domain.Services;
using Inoreader.Domain.Services.Interfaces;

// ReSharper disable ExplicitCallerInfoArgument

namespace handyNews.UWP.ViewModels.Controls
{
    public class SignInDialogViewModel : BindableBase
    {
        #region Fields

        private string _email;
        private string _password;
        private bool _isBusy;
        private string _signInError;

        #endregion

        #region Properties

        public ICredentialService CredentialService { get; set; }
        public ITelemetryManager TelemetryManager { get; set; }

        public string Email
        {
            get { return _email; }
            set
            {
                if (SetProperty(ref _email, value))
                    OnPropertyChanged(nameof(SignInAvailable));
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (SetProperty(ref _password, value))
                    OnPropertyChanged(nameof(SignInAvailable));
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (SetProperty(ref _isBusy, value))
                    OnPropertyChanged(nameof(SignInAvailable));
            }
        }

        public string SignInError
        {
            get { return _signInError; }
            set { SetProperty(ref _signInError, value); }
        }

        public bool SignInAvailable => !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password) && !IsBusy;

        #endregion

        public async Task<bool> SignInAsync()
        {
            if (!SignInAvailable)
                return false;

            SignInError = null;
            IsBusy = true;

            try
            {
                await Task.Delay(3000);
                throw new NotImplementedException("ISignInManager is not implemented");
                //await _apiClient.SignInAsync(Email, Password);

                TelemetryManager.TrackEvent(TelemetryEvents.SignIn);
                CredentialService.Save(Email, Password);
                return true;
            }
            catch (Exception ex)
            {
                TelemetryManager.TrackError(ex);
                SignInError = ex.Message;

                return false;
            }
            finally
            {
                IsBusy = false;
            }            
        }
    }
}
