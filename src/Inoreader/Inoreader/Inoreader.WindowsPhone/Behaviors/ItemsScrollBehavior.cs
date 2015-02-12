using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;

namespace Inoreader.Behaviors
{
	public class ItemsScrollBehavior : DependencyObject, IBehavior
	{
		private Point _checkPoint;

		public DependencyObject AssociatedObject
		{
			get;
			private set;
		}

		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
			"Command", typeof(ICommand), typeof(ItemsScrollBehavior), new PropertyMetadata(default(ICommand)));

		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		private void UpdateCheckPoint()
		{
			var control = (FrameworkElement)AssociatedObject;
			var transform = control.TransformToVisual(Window.Current.Content);

			var p = new Point(Window.Current.Bounds.Width / 2, 10);
			_checkPoint = transform.TransformPoint(p);
		}

		public void Attach(DependencyObject associatedObject)
		{
			AssociatedObject = associatedObject;

			((FrameworkElement)AssociatedObject).SizeChanged += AssociatedObject_SizeChanged;

			var scrollViewer = GetScrollViewer(associatedObject);
			if (scrollViewer != null)
			{
				scrollViewer.ViewChanged += MainPage_ViewChanged;
			}
			else
			{
				var element = associatedObject as FrameworkElement;
				if (element != null)
					element.Loaded += element_Loaded;
			}
		}

		void AssociatedObject_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateCheckPoint();
		}

		public void Detach()
		{
			((FrameworkElement)AssociatedObject).SizeChanged -= AssociatedObject_SizeChanged;

			var scrollViewer = GetScrollViewer(AssociatedObject);
			if (scrollViewer != null)
				scrollViewer.ViewChanged -= MainPage_ViewChanged;
		}

		void MainPage_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			var viewer = (ScrollViewer)sender;

			var q = from lvi in VisualTreeHelper.FindElementsInHostCoordinates(_checkPoint, viewer).OfType<ListViewItem>()
					where lvi.Content != null
					select lvi.Content;

			var items = q.ToArray();

			if (items.Length != 0)
			{
				Debug.Assert(items.Length == 1);

				if (Command != null)
					Command.Execute((items));
			}
		}

		void element_Loaded(object sender, RoutedEventArgs e)
		{
			var element = (DependencyObject)sender;
			var scrollViewer = GetScrollViewer(element);
			if (scrollViewer != null)
				scrollViewer.ViewChanged += MainPage_ViewChanged;

			UpdateCheckPoint();
		}

		private ScrollViewer GetScrollViewer(DependencyObject depObj)
		{
			if (depObj is ScrollViewer) return depObj as ScrollViewer;

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
			{
				var child = VisualTreeHelper.GetChild(depObj, i);

				var result = GetScrollViewer(child);
				if (result != null) return result;
			}
			return null;
		}
	}
}