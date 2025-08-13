using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DesktopApplicationTemplate.UI.Helpers
{
    public class CloseConfirmationHelper
    {
        private readonly ILoggingService? _logger;

        public CloseConfirmationHelper(ILoggingService? logger = null)
        {
            _logger = logger;
        }

        public bool CloseConfirmationSuppressed
        {
            get => SettingsViewModel.CloseConfirmationSuppressed;
            set => SettingsViewModel.CloseConfirmationSuppressed = value;
        }

        public bool Show()
        {
            _logger?.Log("Displaying close confirmation", LogLevel.Debug);
            if (CloseConfirmationSuppressed)
            {
                _logger?.Log("Close confirmation suppressed", LogLevel.Debug);
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

            _logger?.Log(result ? "Close confirmed" : "Close canceled", LogLevel.Debug);
            return result;
        }
    }
}
