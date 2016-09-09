using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using handyNews.UWP.ViewModels.Controls.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace handyNews.UWP.Views.Controls
{
    public sealed partial class FeedTree : UserControl
    {
        private IFeedTreeViewModel _viewModel;

        public IFeedTreeViewModel ViewModel
        {
            get
            {
                if (_viewModel == null)
                {
                    _viewModel = ServiceLocator.Current.GetInstance<IFeedTreeViewModel>();
                }

                return _viewModel;
            }
        }

        public FeedTree()
        {
            InitializeComponent();
        }

        private void SubscriptionsTree_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnNavigatedTo();
        }
    }
}