﻿using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace handyNews.Domain.Views.Controls
{
    public sealed partial class StartTileSquare : UserControl, ITile
    {
        private readonly TaskCompletionSource<int> _taskCompletionSource = new TaskCompletionSource<int>();

        public Task LoadComplete
        {
            get { return _taskCompletionSource.Task; }
        }

        public StartTileSquare(long count)
        {
            InitializeComponent();
            CounterText.Text = count.ToString();
        }

        private void ImageBrush_OnImageOpened(object sender, RoutedEventArgs e)
        {
            _taskCompletionSource.SetResult(0);
        }
    }
}