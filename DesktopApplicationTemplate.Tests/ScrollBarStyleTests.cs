using DesktopApplicationTemplate.UI;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ScrollBarStyleTests
    {
        [Fact]
        [TestCategory("WindowsSafe")]
        public void LightTheme_ScrollBarStyle_HasReducedWidth()
        {
            if (!OperatingSystem.IsWindows())
                return;

            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var dict = new ResourceDictionary { Source = new Uri("pack://application:,,,/DesktopApplicationTemplate.UI;component/Themes/LightTheme.xaml", UriKind.Absolute) };
                    Assert.True(dict.Contains(typeof(System.Windows.Controls.Primitives.ScrollBar)));
                    var style = (Style)dict[typeof(System.Windows.Controls.Primitives.ScrollBar)];
                    var width = style.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == System.Windows.Controls.Primitives.ScrollBar.WidthProperty)?.Value;
                    var height = style.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == System.Windows.Controls.Primitives.ScrollBar.HeightProperty)?.Value;
                    Assert.Equal(8.0, (double)(width ?? 0));
                    Assert.Equal(8.0, (double)(height ?? 0));
                }
                catch (Exception e) { ex = e; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;
            ConsoleTestLogger.LogPass();
        }
    }
}
