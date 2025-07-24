using System.Windows;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.Helpers
{
    public static class SaveConfirmationHelper
    {
        public static void Show()
        {
            if (SettingsViewModel.SaveConfirmationSuppressed)
                return;

            var window = new SaveConfirmationWindow { Owner = Application.Current.MainWindow };
            if (window.ShowDialog() == true && window.DontShowAgain)
            {
                SettingsViewModel.SaveConfirmationSuppressed = true;
                var settingsVm = App.AppHost.Services.GetRequiredService<SettingsViewModel>();
                settingsVm.Save();
            }
        }
    }
}
