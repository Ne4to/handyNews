using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace handyNews.Domain.Services
{
    public static class VisualTreeUtilities
    {
        public static T GetVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            var child = default(T);
            var numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < numVisuals; i++)
            {
                var v = VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                    child = GetVisualChild<T>(v);
                if (child != null)
                    break;
            }
            return child;
        }

        public static T GetVisualParent<T>(DependencyObject control)
            where T : DependencyObject
        {
            var current = control;

            while (current != null)
            {
                var typedControl = current as T;
                if (typedControl != null)
                    return typedControl;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}