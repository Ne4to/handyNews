using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace handyNews.Domain.Views.Controls
{
    public interface ITile
    {
        Task LoadComplete { get; }
    }

    public sealed partial class StartTileWide : UserControl, ITile
    {
        private readonly TaskCompletionSource<int> _taskCompletionSource = new TaskCompletionSource<int>();

        public StartTileWide(long count)
        {
            InitializeComponent();

            CounterText.Text = count.ToString();
        }

        public Task LoadComplete
        {
            get { return _taskCompletionSource.Task; }
        }

        private void ImageBrush_OnImageOpened(object sender, RoutedEventArgs e)
        {
            _taskCompletionSource.SetResult(0);
        }
    }
}