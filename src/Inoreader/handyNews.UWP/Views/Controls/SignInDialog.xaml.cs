using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using handyNews.UWP.ViewModels.Controls.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace handyNews.UWP.Views.Controls
{
    public sealed partial class SignInDialog : ContentDialog
    {
        ISignInDialogViewModel _viewModel;

        public ISignInDialogViewModel ViewModel
        {
            get
            {
                if (_viewModel == null)
                {
                    _viewModel = ServiceLocator.Current.GetInstance<ISignInDialogViewModel>();
                }

                return _viewModel;
            }
        }

        public SignInDialog()
        {
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            try
            {
                args.Cancel = !(await ViewModel.SignInAsync());
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void SignInDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            ViewModel.SetDebugUser();
#endif
        }
    }
}
