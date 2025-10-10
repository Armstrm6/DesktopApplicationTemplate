using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using DesktopApplicationTemplate.Core.Services.Protocols.Scp;

namespace DesktopApplicationTemplate.UI.ViewModels.Scp
{
public class ScpServiceViewModel : ViewModelBase, ILoggingViewModel, INetworkAwareViewModel
    {
        private string _host = string.Empty;
        public string Host
        {
            get => _host;
            set
            {
                if (InputValidators.IsValidHost(value))
                    _host = value;
                OnPropertyChanged();
            }
        }

        private string _port = "22";
        public string Port
        {
            get => _port;
            set
            {
                if (int.TryParse(value, out _))
                    _port = value;
                OnPropertyChanged();
            }
        }

        private string _username = string.Empty;
        public string Username { get => _username; set { _username = value; OnPropertyChanged(); } }

        private string _password = string.Empty;
        public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }

        private string _localPath = string.Empty;
        public string LocalPath { get => _localPath; set { _localPath = value; OnPropertyChanged(); } }

        private string _remotePath = string.Empty;
        public string RemotePath { get => _remotePath; set { _remotePath = value; OnPropertyChanged(); } }

        public ICommand BrowseCommand { get; }
        public ICommand TransferCommand { get; }
        public ICommand SaveCommand { get; }
        private bool _isExportingLogs;
        private readonly AsyncRelayCommand _exportLogCommand;

        private ILoggingService? _logger;
        public ILoggingService? Logger
        {
            get => _logger;
            set
            {
                if (_logger == value) return;
                if (_logger is not null)
                    _logger.LogAdded -= OnLogAdded;
                _logger = value;
                if (_logger is not null)
                    _logger.LogAdded += OnLogAdded;
            }
        }

        public ObservableCollection<LogEntry> Logs { get; } = new();

        private LogLevel _logLevelFilter = LogLevel.Debug;
        public LogLevel LogLevelFilter
        {
            get => _logLevelFilter;
            set
            {
                if (_logLevelFilter == value) return;
                _logLevelFilter = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayLogs));
            }
        }

        public IEnumerable<LogEntry> DisplayLogs => Logs.Where(l => l.Level >= LogLevelFilter);

        public ICommand RefreshLogCommand { get; }
        public ICommand ExportLogCommand { get; }
        public ICommand ClearLogCommand { get; }

        private readonly SaveConfirmationHelper _saveHelper;
        private readonly IScpUploadService _scpUploadService;

        public ScpServiceViewModel(SaveConfirmationHelper saveHelper, IScpUploadService scpUploadService)
        {
            _saveHelper = saveHelper ?? throw new ArgumentNullException(nameof(saveHelper));
            _scpUploadService = scpUploadService ?? throw new ArgumentNullException(nameof(scpUploadService));
            BrowseCommand = new RelayCommand(Browse);
            TransferCommand = new AsyncRelayCommand(TransferAsync);
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            RefreshLogCommand = new RelayCommand(() => OnPropertyChanged(nameof(DisplayLogs)));
            _exportLogCommand = new AsyncRelayCommand(ExportLogsAsync, () => !_isExportingLogs);
            ExportLogCommand = _exportLogCommand;
            ClearLogCommand = new RelayCommand(ClearLogs);
        }

        private void Browse()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            if (dialog.ShowDialog() == true)
                LocalPath = dialog.FileName;
        }

        private async Task TransferAsync()
        {
            if (string.IsNullOrWhiteSpace(LocalPath) || string.IsNullOrWhiteSpace(RemotePath))
                return;
            Logger?.Log("SCP transfer start", LogLevel.Debug);
            await _scpUploadService.UploadAsync(Host, int.Parse(Port), Username, Password, LocalPath, RemotePath);
            Logger?.Log("File transferred", LogLevel.Debug);
            Logger?.Log("SCP transfer finished", LogLevel.Debug);
        }

        private Task SaveAsync() => _saveHelper.ShowAsync();

        public void UpdateNetworkConfiguration(NetworkConfiguration configuration)
        {
            Host = configuration.IpAddress;
        }

        private void ClearLogs()
        {
            Logs.Clear();
            OnPropertyChanged(nameof(DisplayLogs));
            Logger?.Log("SCP logs cleared", LogLevel.Debug);
        }

        private async Task ExportLogsAsync()
        {
            if (_isExportingLogs)
            {
                return;
            }

            _isExportingLogs = true;
            _exportLogCommand.RaiseCanExecuteChanged();

            var path = Path.Combine(Path.GetTempPath(), "scp_logs.txt");

            try
            {
                var lines = DisplayLogs.Select(l => l.Message).ToList();
                await File.WriteAllLinesAsync(path, lines);
                Logger?.Log($"SCP logs exported to {path}", LogLevel.Debug);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Logger?.Log($"Failed to export SCP logs to {path}: {ex.Message}", LogLevel.Error);
            }
            finally
            {
                _isExportingLogs = false;
                _exportLogCommand.RaiseCanExecuteChanged();
            }
        }

        private void OnLogAdded(LogEntry entry) => Logs.Insert(0, entry);

        // OnPropertyChanged provided by ViewModelBase
    }
}
