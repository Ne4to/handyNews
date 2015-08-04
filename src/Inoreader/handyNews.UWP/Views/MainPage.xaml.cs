using System;
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
            Tree.ViewModel.SubscriptionSelected += ViewModel_SubscriptionSelected;


            var signInManager = ServiceLocator.Current.GetInstance<ISignInManager>();
            if (signInManager.SignInRequired)
            {
                SignInDialog dialog = new SignInDialog();
                await dialog.ShowAsync();
            }
        }

        private void ViewModel_SubscriptionSelected(object sender, SubscriptionSelectedEventArgs e)
        {
            ItemsView.ViewModel.UpdateItems(e.Item.Id);            
        }
    }
}
