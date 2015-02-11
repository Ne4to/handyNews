using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Inoreader.Views.Details
{
	public sealed partial class EmptySpaceItem : UserControl
	{
		public EmptySpaceItem()
		{
			this.InitializeComponent();
		}

		private void EmptySpaceItem_OnLoaded(object sender, RoutedEventArgs e)
		{
			var bounds = Window.Current.Bounds;
			Width = bounds.Width / 2;
			Height = bounds.Height * 0.8;

			Window.Current.SizeChanged += Current_SizeChanged;
		}

		void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
		{
			Width = e.Size.Width / 2;
			Height = e.Size.Height * 0.8;
		}
	}
}
