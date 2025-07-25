using System.Windows;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.Helpers
{
    public static class CloseEditorConfirmationHelper
    {
        public static ILoggingService? Logger { get; set; }

        public static bool ConfirmationSuppressed
        {
            get => SettingsViewModel.CloseEditorConfirmationSuppressed;
            set => SettingsViewModel.CloseEditorConfirmationSuppressed = value;
        }

        public static bool Show()
        {
            Logger?.Log("Displaying close editor confirmation", LogLevel.Debug);
            if (ConfirmationSuppressed)
            {
                Logger?.Log("Close confirmation suppressed", LogLevel.Debug);
                return true;
            }

            var window = new CloseEditorConfirmationWindow
            {
                Owner = Application.Current.MainWindow
            };
            if (window.ShowDialog() == true)
            {
                if (window.DontShowAgain)
                {
                    ConfirmationSuppressed = true;
                    var settingsVm = App.AppHost.Services.GetRequiredService<SettingsViewModel>();
                    settingsVm.Save();
                }
                Logger?.Log("Close editor confirmed via dialog", LogLevel.Debug);
                return true;
            }
            Logger?.Log("Close editor cancelled", LogLevel.Debug);
            return false;
        }
    }
}
