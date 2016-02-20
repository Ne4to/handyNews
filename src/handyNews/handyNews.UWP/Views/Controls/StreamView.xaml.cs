using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using handyNews.UWP.ViewModels.Controls.Interfaces;
using Microsoft.Practices.ServiceLocation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace handyNews.UWP.Views.Controls
{
    public sealed partial class StreamView : UserControl
    {
        IStreamViewViewModel _viewModel;

        public IStreamViewViewModel ViewModel
        {
            get
            {
                if (_viewModel == null)
                {
                    _viewModel = ServiceLocator.Current.GetInstance<IStreamViewViewModel>();
                }

                return _viewModel;
            }
        }

        public StreamView()
        {
            this.InitializeComponent();
        }

        private void StreamView_OnLoaded(object sender, RoutedEventArgs e)
        {
           // throw new NotImplementedException();
        }
    }
}
