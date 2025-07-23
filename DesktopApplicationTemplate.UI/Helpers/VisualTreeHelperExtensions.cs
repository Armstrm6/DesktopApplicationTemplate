using System.Windows;
using System.Windows.Media;

namespace DesktopApplicationTemplate.UI.Helpers
{
    public static class VisualTreeHelperExtensions
    {
        public static T? FindParent<T>(DependencyObject? child) where T : DependencyObject
        {
            DependencyObject? parent = child;
            while (parent != null)
            {
                if (parent is T correctlyTyped)
                    return correctlyTyped;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}
