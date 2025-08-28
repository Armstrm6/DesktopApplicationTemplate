using DesktopApplicationTemplate.UI.Services;
using System.Windows;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ThemeManagerTests
    {
        [WpfFact]
        public void ApplyTheme_LoadsResourceDictionary()
        {

            var app = Application.Current ?? new Application();
            try
            {
                ThemeManager.ApplyTheme(true);
                Assert.Contains(app.Resources.MergedDictionaries, d => d.Source?.OriginalString?.Contains("DarkTheme.xaml") == true);
                ThemeManager.ApplyTheme(false);
                Assert.Contains(app.Resources.MergedDictionaries, d => d.Source?.OriginalString?.Contains("LightTheme.xaml") == true);
                ConsoleTestLogger.LogPass();
            }
            finally
            {
                app.Shutdown();
            }
        }
    }
}
