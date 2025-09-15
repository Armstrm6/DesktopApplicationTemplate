using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ILoggingService? _logger;

        internal static string FilePath { get; set; } =
            Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "userSettings.json");
        private readonly ILoggingService? _logger;
        private bool _darkTheme;
        private bool _autoCheckUpdates;
        private bool _runUIOnStartup;
        private bool _runServicesOnStartup;
        private bool _logTcpMessages = true;
        private bool _firstRun = true;
        private ServiceType _preferredServiceType = ServiceType.Tcp;
        private static bool _suppressSaveConfirmation;
        private static bool _suppressCloseConfirmation;
        private bool _dirty;

        public SettingsViewModel(ILoggingService? logger = null)
        {
            _logger = logger;
        }

        public static bool TcpLoggingEnabled { get; private set; } = true;
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
        public bool LogTcpMessages { get => _logTcpMessages; set { _logTcpMessages = value; _dirty = true; OnPropertyChanged(); } }
        public bool FirstRun { get => _firstRun; set { _firstRun = value; _dirty = true; OnPropertyChanged(); } }
        public ServiceType PreferredServiceType { get => _preferredServiceType; set { _preferredServiceType = value; _dirty = true; OnPropertyChanged(); } }
        public ServiceType[] ServiceTypes { get; } = Enum.GetValues<ServiceType>();
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
            if (!File.Exists(FilePath))
            {
                return;
            }

            UserSettings? userSettings = null;

            try
            {
                var json = File.ReadAllText(FilePath);
                userSettings = JsonSerializer.Deserialize<UserSettings>(json);
            }
            catch (IOException ex)
            {
                _logger?.Log($"Failed to load settings from '{FilePath}'. Using default settings. Error: {ex}", LogLevel.Error);
                return;
            }

            if (userSettings == null)
            {
                return;
            }

            _darkTheme = userSettings.DarkTheme;
            _autoCheckUpdates = userSettings.AutoCheckUpdates;
            _runUIOnStartup = userSettings.RunUIOnStartup;
            _runServicesOnStartup = userSettings.RunServicesOnStartup;
            _logTcpMessages = userSettings.LogTcpMessages;
            _firstRun = userSettings.FirstRun;
            _preferredServiceType = userSettings.PreferredServiceType;
            TcpLoggingEnabled = userSettings.LogTcpMessages;
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
                LogTcpMessages = _logTcpMessages,
                FirstRun = _firstRun,
                PreferredServiceType = _preferredServiceType,
                SuppressSaveConfirmation = SaveConfirmationSuppressed,
                SuppressCloseConfirmation = CloseConfirmationSuppressed
            };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            try
            {
                File.WriteAllText(FilePath, JsonSerializer.Serialize(data, options));
                TcpLoggingEnabled = _logTcpMessages;
                _dirty = false;
            }
            catch (StackOverflowException)
            {
                var dumpOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve
                };
                var dump = JsonSerializer.Serialize(data, dumpOptions);
                var temp = Path.Combine(Path.GetTempPath(), "settings_dump.json");
                File.WriteAllText(temp, dump);
                Environment.FailFast($"Stack overflow while saving settings. Dump written to {temp}");
            }
        }

        // OnPropertyChanged provided by ViewModelBase
    }
}
