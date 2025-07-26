using System;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Provides runtime application theme switching.
    /// </summary>
    public static class ThemeManager
    {
        private static ResourceDictionary? _current;

        /// <summary>
        /// Apply the dark or light theme.
        /// </summary>
        /// <param name="useDark">True to apply the dark theme.</param>
        public static void ApplyTheme(bool useDark)
        {
            var logger = App.AppHost?.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            var log = logger?.CreateLogger("ThemeManager");
            try
            {
                log?.LogInformation("Applying {Theme} theme", useDark ? "dark" : "light");
                var themeFile = useDark ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml";
                var uri = new Uri($"pack://application:,,,/DesktopApplicationTemplate.UI;component/{themeFile}", UriKind.Absolute);
                var dict = new ResourceDictionary { Source = uri };
                if (_current != null)
                {
                    System.Windows.Application.Current.Resources.MergedDictionaries.Remove(_current);
                    log?.LogDebug("Removed previous theme dictionary");
                }
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(dict);
                _current = dict;
                log?.LogInformation("Theme applied successfully: {Theme}", themeFile);
            }
            catch (Exception ex)
            {
                log?.LogError(ex, "Failed to apply theme");
            }
        }
    }
}
