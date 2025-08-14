using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        internal static string FilePath { get; set; } =
            Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "userSettings.json");
        private bool _darkTheme;
        private bool _autoCheckUpdates;
        private bool _runUIOnStartup;
        private bool _runServicesOnStartup;
        private bool _logTcpMessages = true;
        private bool _firstRun = true;
        private static bool _suppressSaveConfirmation;
        private static bool _suppressCloseConfirmation;
        private bool _dirty;

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
        public bool HasUnsavedChanges => _dirty;

        public void Load()
        {
            if (!File.Exists(FilePath))
            {
                return;
            }
            var json = File.ReadAllText(FilePath);
            var obj = JsonSerializer.Deserialize<UserSettings>(json);
            if (obj != null)
            {
                _darkTheme = obj.DarkTheme;
                _autoCheckUpdates = obj.AutoCheckUpdates;
                _runUIOnStartup = obj.RunUIOnStartup;
                _runServicesOnStartup = obj.RunServicesOnStartup;
                _logTcpMessages = obj.LogTcpMessages;
                _firstRun = obj.FirstRun;
                TcpLoggingEnabled = obj.LogTcpMessages;
                SaveConfirmationSuppressed = obj.SuppressSaveConfirmation;
                CloseConfirmationSuppressed = obj.SuppressCloseConfirmation;
            }
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
                SuppressSaveConfirmation = SaveConfirmationSuppressed,
                SuppressCloseConfirmation = CloseConfirmationSuppressed
            };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            File.WriteAllText(FilePath, JsonSerializer.Serialize(data, options));
            TcpLoggingEnabled = _logTcpMessages;
            _dirty = false;
        }

        // OnPropertyChanged provided by ViewModelBase
    }
}
