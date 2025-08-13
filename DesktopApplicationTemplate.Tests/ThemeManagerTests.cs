using DesktopApplicationTemplate.UI.Services;
using System.Threading;
using System.Windows;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ThemeManagerTests
    {
        [Fact]
        [TestCategory("WindowsSafe")]
        public void ApplyTheme_LoadsResourceDictionary()
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
                    var app = System.Windows.Application.Current ?? new System.Windows.Application();

                    ThemeManager.ApplyTheme(true);
                    Assert.Contains(app.Resources.MergedDictionaries, d => d.Source?.OriginalString?.Contains("DarkTheme.xaml") == true);
                    ThemeManager.ApplyTheme(false);
                    Assert.Contains(app.Resources.MergedDictionaries, d => d.Source?.OriginalString?.Contains("LightTheme.xaml") == true);
                }
                catch (Exception e)
                {
                    ex = e;
                }
                finally
                {
                    // ensure application instance is cleaned up on the same thread it was created
                    System.Windows.Application.Current?.Shutdown();
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
