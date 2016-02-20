using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using handyNews.Domain.Services;
using Microsoft.Xaml.Interactivity;

namespace Inoreader.Behaviors
{
	[ContentProperty(Name = "Items")]
	public class ItemsScrollBehavior : DependencyObject, IBehavior
	{
		private readonly List<IScrollBehaviorItem> _items = new List<IScrollBehaviorItem>();

		public DependencyObject AssociatedObject
		{
			get;
			private set;
		}

		public List<IScrollBehaviorItem> Items
		{
			get { return _items; }
		}

		public void Attach(DependencyObject associatedObject)
		{
			AssociatedObject = associatedObject;

			((FrameworkElement)AssociatedObject).SizeChanged += AssociatedObject_SizeChanged;

			var scrollViewer = VisualTreeUtilities.GetVisualChild<ScrollViewer>(associatedObject);
			if (scrollViewer != null)
			{
				foreach (var item in Items)
				{
					item.ParentElement = (FrameworkElement)AssociatedObject;
					item.ScrollViewer = scrollViewer;
				}

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
			foreach (var item in Items)
			{
				item.OnSizeChanged();
			}
		}

		public void Detach()
		{
			((FrameworkElement)AssociatedObject).SizeChanged -= AssociatedObject_SizeChanged;

			var scrollViewer = VisualTreeUtilities.GetVisualChild<ScrollViewer>(AssociatedObject);
			if (scrollViewer != null)
				scrollViewer.ViewChanged -= MainPage_ViewChanged;
		}

		void MainPage_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			foreach (var item in Items)
			{
				item.OnScrollChanged();
			}
		}

		void element_Loaded(object sender, RoutedEventArgs e)
		{
			var element = (DependencyObject)sender;
			var scrollViewer = VisualTreeUtilities.GetVisualChild<ScrollViewer>(element);
			if (scrollViewer != null)
				scrollViewer.ViewChanged += MainPage_ViewChanged;

			foreach (var item in Items)
			{
				item.ParentElement = (FrameworkElement)sender;
				item.ScrollViewer = scrollViewer;

				item.OnSizeChanged();
			}
		}
	}

	public interface IScrollBehaviorItem
	{
		FrameworkElement ParentElement { get; set; }
		ScrollViewer ScrollViewer { get; set; }
		void OnSizeChanged();
		void OnScrollChanged();
	}

	public class ScrollBehaviorCommandItem : DependencyObject, IScrollBehaviorItem
	{
		private Point _checkPoint;

		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
			"Command", typeof(ICommand), typeof(ScrollBehaviorCommandItem), new PropertyMetadata(default(ICommand)));

		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		private void UpdateCheckPoint()
		{
			var control = ParentElement;
			if (control == null)
				return;

			var transform = control.TransformToVisual(Window.Current.Content);

			var p = new Point(Window.Current.Bounds.Width / 2, 10);
			_checkPoint = transform.TransformPoint(p);
		}

		public FrameworkElement ParentElement { get; set; }
		public ScrollViewer ScrollViewer { get; set; }

		public void OnSizeChanged()
		{
			UpdateCheckPoint();
		}

		public void OnScrollChanged()
		{
			var viewer = ScrollViewer;

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
	}

	public class ScrollBehaviorCustomItem : DependencyObject, IScrollBehaviorItem
	{
		public FrameworkElement ParentElement { get; set; }
		public ScrollViewer ScrollViewer { get; set; }

		public double OffsetThreshold { get; set; }
		public string LessThresholdStateName { get; set; }
		public string GreateOrEqualThresholdStateName { get; set; }

		public static readonly DependencyProperty ControlProperty = DependencyProperty.Register(
			"Control", typeof (object), typeof (ScrollBehaviorCustomItem), new PropertyMetadata(default(object)));

		public object Control
		{
			get { return (object) GetValue(ControlProperty); }
			set { SetValue(ControlProperty, value); }
		}

		public void OnSizeChanged()
		{
		}

		public void OnScrollChanged()
		{
			if (ParentElement == null || ScrollViewer == null)
				return;

			var control = Control as Control;
			if (control == null)
				return;
			
			var currentOffset = ScrollViewer.VerticalOffset;
			var newState = currentOffset >= OffsetThreshold ? GreateOrEqualThresholdStateName : LessThresholdStateName;

			if (!string.IsNullOrEmpty(newState))
			{
				var res = VisualStateManager.GoToState(control, newState, true);
			}
		}
	}
}