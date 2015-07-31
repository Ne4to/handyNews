using Windows.UI.Xaml.Controls;
using handyNews.UWP.ViewModels.Controls;
using Microsoft.Practices.ServiceLocation;

namespace handyNews.UWP.Views.Controls
{
    public sealed partial class SignInDialog : ContentDialog
    {
        SignInDialogViewModel _viewModel;

        public SignInDialogViewModel ViewModel
        {
            get
            {
                if (_viewModel == null)
                {
                    _viewModel = ServiceLocator.Current.GetInstance<SignInDialogViewModel>();
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
    }
}
