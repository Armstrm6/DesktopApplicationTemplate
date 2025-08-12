using System;
using System.Threading;
using System.Windows;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ThemeResourceTests
    {
        [Fact]
        [TestCategory("WindowsSafe")]
        public void ThemeResources_Load()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var dict = new ResourceDictionary
                    {
                        Source = new Uri("/DesktopApplicationTemplate.UI;component/Themes/Theme.xaml", UriKind.Relative)
                    };
                    Assert.NotNull(dict["Color0Brush"]);
                }
                catch (Exception e)
                {
                    ex = e;
                }
                finally
                {
                    Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;
            ConsoleTestLogger.LogPass();
        }
    }
}
