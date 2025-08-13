using System.Windows;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.Helpers
{
    public static class SaveConfirmationHelper
    {
        public static ILoggingService? Logger { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the save confirmation dialog
        /// should be suppressed. This simply forwards to
        /// <see cref="SettingsViewModel.SaveConfirmationSuppressed"/> so callers
        /// do not need to depend on <see cref="SettingsViewModel"/> directly.
        /// </summary>
        public static bool SaveConfirmationSuppressed
        {
            get => SettingsViewModel.SaveConfirmationSuppressed;
            set => SettingsViewModel.SaveConfirmationSuppressed = value;
        }

        public static event Action? SaveConfirmed;

        public static void Show()
        {
            Logger?.Log("Displaying save confirmation", LogLevel.Debug);
            if (SaveConfirmationSuppressed)
            {
                Logger?.Log("Confirmation suppressed", LogLevel.Debug);
                SaveConfirmed?.Invoke();
                Logger?.Log("Save confirmed via suppression", LogLevel.Debug);
                return;
            }

            var window = new SaveConfirmationWindow
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            if (window.ShowDialog() == true)
            {
                if (window.DontShowAgain)
                {
                    SaveConfirmationSuppressed = true;
                    var settingsVm = App.AppHost.Services.GetRequiredService<SettingsViewModel>();
                    settingsVm.Save();
                }

                SaveConfirmed?.Invoke();
                Logger?.Log("Save confirmed via dialog", LogLevel.Debug);
            }
        }
    }
}
