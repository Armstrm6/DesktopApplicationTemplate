using DesktopApplicationTemplate.UI;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ScrollBarStyleTests
    {
        [WindowsFact]
        public void LightTheme_ScrollBarStyle_HasReducedWidth()
        {
            ApplicationResourceHelper.RunOnDispatcher(() =>
            {
                var dict = new ResourceDictionary { Source = new Uri("pack://application:,,,/DesktopApplicationTemplate.UI;component/Themes/LightTheme.xaml") };
                Assert.True(dict.Contains(typeof(ScrollBar)));
                var style = (Style)dict[typeof(ScrollBar)];
                var width = style.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == ScrollBar.WidthProperty)?.Value;
                var height = style.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == ScrollBar.HeightProperty)?.Value;
                Assert.Equal(8.0, (double)(width ?? 0));
                Assert.Equal(8.0, (double)(height ?? 0));
                ConsoleTestLogger.LogPass();
            });
        }
    }
}
