using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Inoreader.Views.Controls
{
	public sealed class StreamListViewControl : Control
	{
		private bool _expanded;

		public static readonly DependencyProperty HtmlContentProperty = DependencyProperty.Register(
			"HtmlContent", typeof (object), typeof (StreamListViewControl), new PropertyMetadata(default(object)));

		public object HtmlContent
		{
			get { return (object) GetValue(HtmlContentProperty); }
			set { SetValue(HtmlContentProperty, value); }
		}

		public StreamListViewControl()
		{
			this.DefaultStyleKey = typeof(StreamListViewControl);
			this.IsTapEnabled = true;
			Tapped += StreamListViewControl_Tapped;
		}

		void StreamListViewControl_Tapped(object sender, TappedRoutedEventArgs e)
		{
			var newState = _expanded ? "CollapsedState" : "ExpandedState";
			_expanded = !_expanded;
			VisualStateManager.GoToState(this, newState, false);
			e.Handled = true;
		}
	}
}
