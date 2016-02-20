using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using handyNews.Domain.Services;

namespace handyNews.Domain.Utils
{
	public static class ScrollUtils
	{
		public static void ScrollItem(ListViewBase control, int indexDelta)
		{
			if (control == null || control.Items == null)
				return;

			var scrollViewer = VisualTreeUtilities.GetVisualChild<ScrollViewer>(control);

			var p = new Point(Window.Current.Bounds.Width/2, 10);
			var transform = control.TransformToVisual(Window.Current.Content);
			var checkPoint = transform.TransformPoint(p);

			var q = from lvi in VisualTreeHelper.FindElementsInHostCoordinates(checkPoint, scrollViewer).OfType<ListViewItem>()
				where lvi.Content != null
				select lvi.Content;

			var item = q.FirstOrDefault();

			if (item == null) 
				return;

			var index = control.Items.IndexOf(item);
			var nextItemIndex = index + indexDelta;
			if (index != -1 && nextItemIndex >= 0 && nextItemIndex < control.Items.Count)
			{					
				var nextItem = control.Items[nextItemIndex];
				control.ScrollIntoView(nextItem, ScrollIntoViewAlignment.Leading);
			}
		}
	}
}