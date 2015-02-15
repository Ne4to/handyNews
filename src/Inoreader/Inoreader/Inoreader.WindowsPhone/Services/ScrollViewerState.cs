using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Inoreader.Annotations;

namespace Inoreader.Services
{
	public class ScrollViewerState
	{
		const double Tolerance = 1D;

		private readonly FrameworkElement _element;

		private ScrollViewer _scrollViewer;

		private double _scrollableWidth;
		private double _scrollableHeight;
		private double _horizontalOffset;
		private double _verticalOffset;

		private bool _loaded;
		private bool _verticalSet;
		private bool _horizontalSet;

		public ScrollViewerState([NotNull] FrameworkElement element)
		{
			if (element == null) throw new ArgumentNullException("element");
			_element = element;
			_element.LayoutUpdated += _element_LayoutUpdated;
		}

		public void Save([NotNull] Dictionary<string, object> pageState, string keyPrefix)
		{
			if (pageState == null) throw new ArgumentNullException("pageState");

			if (_scrollViewer == null)
			{
				_scrollViewer = VisualTreeUtilities.GetVisualChild<ScrollViewer>(_element);
				if (_scrollViewer == null)
					return;
			}

			pageState[keyPrefix + "ScrollableWidth"] = _scrollViewer.ScrollableWidth;
			pageState[keyPrefix + "ScrollableHeight"] = _scrollViewer.ScrollableHeight;
			pageState[keyPrefix + "HorizontalOffset"] = _scrollViewer.HorizontalOffset;
			pageState[keyPrefix + "VerticalOffset"] = _scrollViewer.VerticalOffset;
		}

		public void Load([NotNull] Dictionary<string, object> pageState, string keyPrefix)
		{
			if (pageState == null) throw new ArgumentNullException("pageState");

			_scrollableWidth = pageState.GetValue<double>(keyPrefix + "ScrollableWidth");
			_scrollableHeight = pageState.GetValue<double>(keyPrefix + "ScrollableHeight");
			_horizontalOffset = pageState.GetValue<double>(keyPrefix + "HorizontalOffset");
			_verticalOffset = pageState.GetValue<double>(keyPrefix + "VerticalOffset");

			_loaded = true;
		}

		void _element_LayoutUpdated(object sender, object e)
		{
			if (!_loaded)
				return;

			if (_scrollViewer == null)
			{
				_scrollViewer = VisualTreeUtilities.GetVisualChild<ScrollViewer>(_element);
				if (_scrollViewer == null)
					return;
			}

			var canSetHorizontalOffset = Math.Abs(_scrollViewer.ScrollableWidth - _scrollableWidth) < Tolerance;
			var canSetVerticalOffset = Math.Abs(_scrollViewer.ScrollableHeight - _scrollableHeight) < Tolerance;

			if (canSetHorizontalOffset && !_horizontalSet)
			{
				_scrollViewer.ChangeView(_horizontalOffset, null, null, true);
				_horizontalSet = true;
			}

			if (canSetVerticalOffset && !_verticalSet)
			{
				_scrollViewer.ChangeView(null, _verticalOffset, null, true);
				_verticalSet = true;
			}

			if (_verticalSet && _horizontalSet)
			{
				_element.LayoutUpdated -= _element_LayoutUpdated;
			}
		}
	}
}