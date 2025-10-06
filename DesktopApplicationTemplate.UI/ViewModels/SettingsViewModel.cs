using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        internal static string FilePath => UserSettingsStorage.FilePath;
        private readonly ILoggingService? _logger;
        private bool _darkTheme;
        private bool _autoCheckUpdates;
        private bool _runUIOnStartup;
        private bool _runServicesOnStartup;
        private bool _firstRun = true;
        private static bool _suppressSaveConfirmation;
        private static bool _suppressCloseConfirmation;
        private bool _dirty;

        public static bool SaveConfirmationSuppressed
        {
            get => _suppressSaveConfirmation;
            internal set => _suppressSaveConfirmation = value;
        }

        public static bool CloseConfirmationSuppressed
        {
            get => _suppressCloseConfirmation;
            internal set => _suppressCloseConfirmation = value;
        }

        public bool DarkTheme { get => _darkTheme; set { _darkTheme = value; _dirty = true; OnPropertyChanged(); } }
        public bool AutoCheckUpdates { get => _autoCheckUpdates; set { _autoCheckUpdates = value; _dirty = true; OnPropertyChanged(); } }
        public bool RunUIOnStartup { get => _runUIOnStartup; set { _runUIOnStartup = value; _dirty = true; OnPropertyChanged(); } }
        public bool RunServicesOnStartup { get => _runServicesOnStartup; set { _runServicesOnStartup = value; _dirty = true; OnPropertyChanged(); } }
        public bool FirstRun { get => _firstRun; set { _firstRun = value; _dirty = true; OnPropertyChanged(); } }
        public bool HasUnsavedChanges => _dirty;

        public SettingsViewModel()
            : this(null)
        {
        }

        public SettingsViewModel(ILoggingService? logger)
        {
            _logger = logger;
        }

        public void Load()
        {
            var userSettings = UserSettingsStorage.Load(_logger);

            _darkTheme = userSettings.DarkTheme;
            _autoCheckUpdates = userSettings.AutoCheckUpdates;
            _runUIOnStartup = userSettings.RunUIOnStartup;
            _runServicesOnStartup = userSettings.RunServicesOnStartup;
            _firstRun = userSettings.FirstRun;
            SaveConfirmationSuppressed = userSettings.SuppressSaveConfirmation;
            CloseConfirmationSuppressed = userSettings.SuppressCloseConfirmation;
        }

        public void Save()
        {
            var data = new UserSettings
            {
                DarkTheme = _darkTheme,
                AutoCheckUpdates = _autoCheckUpdates,
                RunUIOnStartup = _runUIOnStartup,
                RunServicesOnStartup = _runServicesOnStartup,
                FirstRun = _firstRun,
                SuppressSaveConfirmation = SaveConfirmationSuppressed,
                SuppressCloseConfirmation = CloseConfirmationSuppressed
            };
            UserSettingsStorage.Save(data, _logger);
            _dirty = false;
        }

        // OnPropertyChanged provided by ViewModelBase
    }
}
