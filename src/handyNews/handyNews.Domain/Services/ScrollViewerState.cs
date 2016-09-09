using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using handyNews.Domain.Utils;
using JetBrains.Annotations;

namespace handyNews.Domain.Services
{
    public class ScrollViewerState
    {
        private readonly FrameworkElement _element;
        private double _horizontalOffset;
        private bool _horizontalSet;

        private bool _loaded;
        private double _scrollableHeight;

        private double _scrollableWidth;

        private ScrollViewer _scrollViewer;
        private double _verticalOffset;
        private bool _verticalSet;
        private const double Tolerance = 0.05D;

        public ScrollViewerState([NotNull] FrameworkElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            _element = element;
            _element.LayoutUpdated += _element_LayoutUpdated;
        }

        public void Save([NotNull] Dictionary<string, object> pageState, string keyPrefix)
        {
            if (pageState == null)
            {
                throw new ArgumentNullException("pageState");
            }

            if (_scrollViewer == null)
            {
                _scrollViewer = VisualTreeUtilities.GetVisualChild<ScrollViewer>(_element);
                if (_scrollViewer == null)
                {
                    return;
                }
            }

            pageState[keyPrefix + "ScrollableWidth"] = _scrollViewer.ScrollableWidth;
            pageState[keyPrefix + "ScrollableHeight"] = _scrollViewer.ScrollableHeight;
            pageState[keyPrefix + "HorizontalOffset"] = _scrollViewer.HorizontalOffset;
            pageState[keyPrefix + "VerticalOffset"] = _scrollViewer.VerticalOffset;
        }

        public void Load([NotNull] Dictionary<string, object> pageState, string keyPrefix)
        {
            if (pageState == null)
            {
                throw new ArgumentNullException("pageState");
            }

            _scrollableWidth = pageState.GetValue<double>(keyPrefix + "ScrollableWidth");
            _scrollableHeight = pageState.GetValue<double>(keyPrefix + "ScrollableHeight");
            _horizontalOffset = pageState.GetValue<double>(keyPrefix + "HorizontalOffset");
            _verticalOffset = pageState.GetValue<double>(keyPrefix + "VerticalOffset");

            _loaded = true;
        }

        private void _element_LayoutUpdated(object sender, object e)
        {
            if (!_loaded)
            {
                return;
            }

            if (_scrollViewer == null)
            {
                _scrollViewer = VisualTreeUtilities.GetVisualChild<ScrollViewer>(_element);
                if (_scrollViewer == null)
                {
                    return;
                }
            }

            var canSetHorizontalOffset = (_scrollViewer.ScrollableWidth > 0D)
                && (_scrollableWidth > 0D)
                && (Math.Abs(1D - _scrollViewer.ScrollableWidth/_scrollableWidth) < Tolerance);

            if (canSetHorizontalOffset && !_horizontalSet)
            {
                _scrollViewer.ChangeView(_horizontalOffset, null, null, true);
                _horizontalSet = true;
            }

            var canSetVerticalOffset = (_scrollViewer.ScrollableHeight > 0D)
                && (_scrollableHeight > 0D)
                && (Math.Abs(1D - _scrollViewer.ScrollableHeight/_scrollableHeight) < Tolerance);

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