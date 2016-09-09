using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using handyNews.UWP.ViewModels;
using Microsoft.Practices.ServiceLocation;

namespace handyNews.UWP.Views
{
    public sealed partial class MainPage : Page
    {
        private MainPageViewModel _viewModel;

        public MainPageViewModel ViewModel
        {
            get
            {
                if (DesignMode.DesignModeEnabled)
                {
                    return null;
                }

                if (_viewModel == null)
                {
                    _viewModel = ServiceLocator.Current.GetInstance<MainPageViewModel>();
                }

                return _viewModel;
            }
        }

        public MainPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var manager = SystemNavigationManager.GetForCurrentView();
            manager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            manager.BackRequested += Manager_BackRequested;

            //if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            //{
            //    Windows.Phone.UI.Input.HardwareButtons.BackPressed += (s, a) =>
            //    {
            //        Debug.WriteLine("BackPressed");
            //        if (Frame.CanGoBack)
            //        {
            //            Frame.GoBack();
            //            a.Handled = true;
            //        }
            //    };
            //}

            ViewModel.OnNavigatedTo();
        }

        private void Manager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Tree.ViewModel.ShowRoot();
            if (ViewModel.ShowStreamView)
            {
                ViewModel.ShowStreamView = false;
            }
        }
    }
}