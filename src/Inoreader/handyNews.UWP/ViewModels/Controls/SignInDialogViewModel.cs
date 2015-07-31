using handyNews.UWP.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Inoreader.Domain.Services;
using ReactiveUI;
// ReSharper disable ExplicitCallerInfoArgument

namespace handyNews.UWP.ViewModels.Controls
{
    public class SignInDialogViewModel : BindableBase
    {
        #region Fields

        private string _email;
        private string _password;
        private bool _isBusy;

        #endregion

        #region Properties

        public ICredentialService CredentialService { get; set; }
        
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

        public bool SignInAvailable => !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password) && !IsBusy;

        #endregion

        public async Task<bool> SignInAsync()
        {
            if (!SignInAvailable)
                return false;

            IsBusy = true;

            Exception error = null;

            try
            {
                await Task.Delay(3000);
                //await _apiClient.SignInAsync(Email, Password);

                //_telemetryClient.TrackEvent(TelemetryEvents.SignIn);

                CredentialService.Save(Email, Password);

                //_navigationService.Navigate(PageTokens.Subscriptions, null);
                return true;
            }
            catch (Exception ex)
            {
                error = ex;
                //_telemetryClient.TrackExceptionFull(ex);

                return false;
            }
            finally
            {
                IsBusy = false;
            }

            //if (error != null)
            //{
            //    MessageDialog msgbox = new MessageDialog(error.Message, Strings.Resources.ErrorDialogTitle);
            //    await msgbox.ShowAsync();
            //}
        }
    }
}
