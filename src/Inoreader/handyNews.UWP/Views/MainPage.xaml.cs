using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using handyNews.UWP.ViewModels.Controls.Interfaces;
using handyNews.UWP.Views.Controls;
using Inoreader.Domain.Services.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace handyNews.UWP.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
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

            Tree.ViewModel.SubscriptionSelected += ViewModel_SubscriptionSelected;
            
            var signInManager = ServiceLocator.Current.GetInstance<ISignInManager>();
            if (signInManager.SignInRequired)
            {
                SignInDialog dialog = new SignInDialog();
                await dialog.ShowAsync();
            }
        }

        private void Manager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Tree.ViewModel.ShowRoot();
        }

        private void ViewModel_SubscriptionSelected(object sender, SubscriptionSelectedEventArgs e)
        {
            ItemsView.ViewModel.UpdateItems(e.Item.Id);            
        }
    }
}
