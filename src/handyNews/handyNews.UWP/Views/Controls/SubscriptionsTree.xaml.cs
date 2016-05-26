using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using handyNews.UWP.ViewModels.Controls.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace handyNews.UWP.Views.Controls
{
    public sealed partial class SubscriptionsTree : UserControl
    {
        private ISubscriptionsTreeViewModel _viewModel;

        public SubscriptionsTree()
        {
            InitializeComponent();
        }

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

        private void SubscriptionsTree_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnNavigatedTo();
        }
    }
}