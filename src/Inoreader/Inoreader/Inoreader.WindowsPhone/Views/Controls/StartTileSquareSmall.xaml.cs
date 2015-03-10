using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Inoreader.Views.Controls
{
	public sealed partial class StartTileSquareSmall : UserControl, ITile
	{
		readonly TaskCompletionSource<int> _taskCompletionSource = new TaskCompletionSource<int>();

		public Task LoadComplete
		{
			get { return _taskCompletionSource.Task; }
		}

		public StartTileSquareSmall(int count)
		{
			this.InitializeComponent();
			CounterText.Text = count.ToString();
		}

		private void ImageBrush_OnImageOpened(object sender, RoutedEventArgs e)
		{
			_taskCompletionSource.SetResult(0);
		}
	}
}
