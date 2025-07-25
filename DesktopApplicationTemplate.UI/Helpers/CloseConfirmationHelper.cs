using System.Windows;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.Helpers
{
    public static class CloseConfirmationHelper
    {
        public static ILoggingService? Logger { get; set; }

        public static bool CloseConfirmationSuppressed
        {
            get => SettingsViewModel.CloseConfirmationSuppressed;
            set => SettingsViewModel.CloseConfirmationSuppressed = value;
        }

        public static bool Show()
        {
            Logger?.Log("Displaying close confirmation", LogLevel.Debug);
            if (CloseConfirmationSuppressed)
            {
                Logger?.Log("Close confirmation suppressed", LogLevel.Debug);
                return true;
            }

            var window = new CloseConfirmationWindow
            {
                Owner = Application.Current.MainWindow
            };
            var result = window.ShowDialog() == true;
            if (result && window.DontShowAgain)
            {
                CloseConfirmationSuppressed = true;
                var settingsVm = App.AppHost.Services.GetRequiredService<SettingsViewModel>();
                settingsVm.Save();
            }

            Logger?.Log(result ? "Close confirmed" : "Close canceled", LogLevel.Debug);
            return result;
        }
    }
}
