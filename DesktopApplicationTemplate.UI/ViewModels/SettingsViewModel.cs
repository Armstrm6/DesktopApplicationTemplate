using System;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Services;
using UI = DesktopApplicationTemplate.UI;

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

        public bool DarkTheme
        {
            get => _darkTheme;
            set
            {
                _darkTheme = value;
                MarkDirty();
                OnPropertyChanged();
            }
        }

        public bool AutoCheckUpdates
        {
            get => _autoCheckUpdates;
            set
            {
                _autoCheckUpdates = value;
                MarkDirty();
                OnPropertyChanged();
            }
        }

        public bool RunUIOnStartup
        {
            get => _runUIOnStartup;
            set
            {
                _runUIOnStartup = value;
                MarkDirty();
                OnPropertyChanged();
            }
        }

        public bool RunServicesOnStartup
        {
            get => _runServicesOnStartup;
            set
            {
                _runServicesOnStartup = value;
                MarkDirty();
                OnPropertyChanged();
            }
        }

        public bool FirstRun
        {
            get => _firstRun;
            set
            {
                _firstRun = value;
                MarkDirty();
                OnPropertyChanged();
            }
        }
        public bool HasUnsavedChanges => _dirty;

        public SettingsViewModel()
            : this(null)
        {
        }

        public SettingsViewModel(ILoggingService? logger)
        {
            _logger = logger;
        }

        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            var userSettings = await UserSettingsStorage.LoadAsync(_logger, cancellationToken).ConfigureAwait(false);

            await SwitchToUiThreadAsync().ConfigureAwait(false);

            _darkTheme = userSettings.DarkTheme;
            _autoCheckUpdates = userSettings.AutoCheckUpdates;
            _runUIOnStartup = userSettings.RunUIOnStartup;
            _runServicesOnStartup = userSettings.RunServicesOnStartup;
            _firstRun = userSettings.FirstRun;
            SaveConfirmationSuppressed = userSettings.SuppressSaveConfirmation;
            CloseConfirmationSuppressed = userSettings.SuppressCloseConfirmation;

            _dirty = false;
            OnPropertyChanged(nameof(DarkTheme));
            OnPropertyChanged(nameof(AutoCheckUpdates));
            OnPropertyChanged(nameof(RunUIOnStartup));
            OnPropertyChanged(nameof(RunServicesOnStartup));
            OnPropertyChanged(nameof(FirstRun));
            OnPropertyChanged(nameof(HasUnsavedChanges));
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await SwitchToUiThreadAsync().ConfigureAwait(false);

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

            await UserSettingsStorage.SaveAsync(data, _logger, cancellationToken).ConfigureAwait(false);

            await SwitchToUiThreadAsync().ConfigureAwait(false);

            _dirty = false;
            OnPropertyChanged(nameof(HasUnsavedChanges));
        }

        private static async Task SwitchToUiThreadAsync()
        {
            if (UI.App.UiThreadTaskFactory is { } factory)
            {
                await factory.SwitchToMainThreadAsync();
            }
        }

        private void MarkDirty()
        {
            if (_dirty)
            {
                return;
            }

            _dirty = true;
            OnPropertyChanged(nameof(HasUnsavedChanges));
        }

        // OnPropertyChanged provided by ViewModelBase
    }
}
