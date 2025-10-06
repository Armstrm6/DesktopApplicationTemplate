using System;
using System.Linq;
using System.Windows;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;

namespace DesktopApplicationTemplate.UI.Services
{
    public interface IStartupPreferencesService
    {
        StartupPreferencesResult ShowDialog(bool runServicesOnStartup, bool runUiOnStartup);
    }

    public readonly record struct StartupPreferencesResult(bool Accepted, bool RunServicesOnStartup, bool RunUIOnStartup);

    /// <summary>
    /// Displays a dialog allowing the user to confirm startup behavior for services and UI.
    /// </summary>
    public sealed class StartupPreferencesDialogService : IStartupPreferencesService
    {
        public StartupPreferencesResult ShowDialog(bool runServicesOnStartup, bool runUiOnStartup)
        {
            var viewModel = new StartupPreferencesViewModel(runServicesOnStartup, runUiOnStartup);
            var window = new StartupPreferencesWindow(viewModel)
            {
                Owner = GetActiveWindow()
            };

            var accepted = window.ShowDialog() == true;
            return new StartupPreferencesResult(accepted, viewModel.RunServicesOnStartup, viewModel.RunUIOnStartup);
        }

        private static Window? GetActiveWindow()
        {
            if (Application.Current is null)
            {
                return null;
            }

            return Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? Application.Current.MainWindow;
        }
    }
}

