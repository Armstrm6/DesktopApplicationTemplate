using System;
using System.Windows;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.Helpers
{
    public class SaveConfirmationHelper
    {
        private readonly ILoggingService? _logger;

        public SaveConfirmationHelper(ILoggingService? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the save confirmation dialog
        /// should be suppressed. This forwards to <see cref="SettingsViewModel.SaveConfirmationSuppressed"/>.
        /// </summary>
        public bool SaveConfirmationSuppressed
        {
            get => SettingsViewModel.SaveConfirmationSuppressed;
            set => SettingsViewModel.SaveConfirmationSuppressed = value;
        }

        public event Action? SaveConfirmed;

        public void Show()
        {
            _logger?.Log("Displaying save confirmation", LogLevel.Debug);
            if (SaveConfirmationSuppressed)
            {
                _logger?.Log("Confirmation suppressed", LogLevel.Debug);
                SaveConfirmed?.Invoke();
                _logger?.Log("Save confirmed via suppression", LogLevel.Debug);
                return;
            }

            var window = new SaveConfirmationWindow
            {
                Owner = Application.Current.MainWindow
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
                _logger?.Log("Save confirmed via dialog", LogLevel.Debug);
            }
        }
    }
}
