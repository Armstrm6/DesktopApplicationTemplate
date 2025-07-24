using System.Windows;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.Helpers
{
    public static class SaveConfirmationHelper
    {
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

        public static void Show()
        {
            if (SaveConfirmationSuppressed)
                return;

            var window = new SaveConfirmationWindow
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            if (window.ShowDialog() == true && window.DontShowAgain)
            {
                SaveConfirmationSuppressed = true;
                var settingsVm = App.AppHost.Services.GetRequiredService<SettingsViewModel>();
                settingsVm.Save();
            }
        }
    }
}
