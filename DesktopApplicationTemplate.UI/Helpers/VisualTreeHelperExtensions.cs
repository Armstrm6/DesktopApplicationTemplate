using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

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

                parent = parent switch
                {
                    FrameworkElement fe => fe.Parent,
                    FrameworkContentElement fce => fce.Parent,
                    ContentElement ce => ContentOperations.GetParent(ce),
                    Visual or Visual3D => VisualTreeHelper.GetParent(parent),
                    _ => LogicalTreeHelper.GetParent(parent)
                };
            }

            return null;
        }
    }
}
