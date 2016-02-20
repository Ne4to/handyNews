using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Inoreader.Views.Details
{
	public sealed partial class HeaderSpaceItem : UserControl
	{
		private bool _heightSet;

		public static readonly DependencyProperty ControlProperty = DependencyProperty.Register(
			"Control", typeof (object), typeof (HeaderSpaceItem), new PropertyMetadata(default(object), OnControlPropertyChanged));

		private static void OnControlPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((HeaderSpaceItem) d).OnControlChanged();
		}

		private void OnControlChanged()
		{
			var control = Control as FrameworkElement;
			if (control == null)
				return;

			if (control.ActualHeight > 0)
			{
				Height = control.ActualHeight;
				_heightSet = true;
			}	
		}

		public object Control
		{
			get { return (object) GetValue(ControlProperty); }
			set { SetValue(ControlProperty, value); }
		}

		public HeaderSpaceItem()
		{
			this.InitializeComponent();
		}

		private void HeaderSpaceItem_OnLoaded(object sender, RoutedEventArgs e)
		{
			if (_heightSet)
				return;

			var bounds = Window.Current.Bounds;
			Width = bounds.Width / 2;
			Height = bounds.Height * 0.1;
		}
	}
}
