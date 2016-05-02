using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using handyNews.UWP.Events;
using handyNews.UWP.ViewModels.Controls.Interfaces;
using Microsoft.Practices.ServiceLocation;
using PubSub;

namespace handyNews.UWP.Views.Controls
{
    public sealed partial class SubscriptionsTree : UserControl
    {
        ISubscriptionsTreeViewModel _viewModel;

        public ISubscriptionsTreeViewModel ViewModel
        {
            get
            {
                if (_viewModel == null)
                {
                    _viewModel = ServiceLocator.Current.GetInstance<ISubscriptionsTreeViewModel>();
                }

                return _viewModel;
            }
        }

        public SubscriptionsTree()
        {
            this.InitializeComponent();
        }

        private void SubscriptionsTree_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnNavigatedTo();
        }
    }
}
