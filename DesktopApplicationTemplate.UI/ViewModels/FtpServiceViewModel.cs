using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    /// <summary>
    /// View model that tracks FTP server transfers and status.
    /// </summary>
    public class FtpServiceViewModel : ViewModelBase, ILoggingViewModel
    {
        private readonly IFtpServerService _ftpServerService;
        private readonly AsyncRelayCommand _startCommand;
        private readonly AsyncRelayCommand _stopCommand;
        private bool _isServerRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpServiceViewModel"/> class.
        /// </summary>
        /// <param name="ftpServerService">Service hosting the FTP server.</param>
        /// <param name="logger">Optional logging service.</param>
        public FtpServiceViewModel(IFtpServerService ftpServerService, ILoggingService? logger = null)
        {
            _ftpServerService = ftpServerService ?? throw new ArgumentNullException(nameof(ftpServerService));
            Logger = logger;

            _ftpServerService.FileReceived += HandleFileReceived;
            _ftpServerService.FileSent += HandleFileSent;

            _startCommand = new AsyncRelayCommand(StartAsync, () => !IsServerRunning);
            _stopCommand = new AsyncRelayCommand(StopAsync, () => IsServerRunning);
        }

        /// <summary>Files uploaded to the server.</summary>
        public ObservableCollection<FtpTransferEventArgs> UploadedFiles { get; } = new();

        /// <summary>Files downloaded from the server.</summary>
        public ObservableCollection<FtpTransferEventArgs> DownloadedFiles { get; } = new();

        /// <inheritdoc />
        public ILoggingService? Logger { get; set; }

        /// <summary>Indicates whether the server is running.</summary>
        public bool IsServerRunning
        {
            get => _isServerRunning;
            private set
            {
                if (_isServerRunning == value)
                    return;
                _isServerRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ServerStatus));
                _startCommand.RaiseCanExecuteChanged();
                _stopCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>Human readable server status.</summary>
        public string ServerStatus => IsServerRunning ? "Running" : "Stopped";

        /// <summary>Command to start the server.</summary>
        public ICommand StartServerCommand => _startCommand;

        /// <summary>Command to stop the server.</summary>
        public ICommand StopServerCommand => _stopCommand;

        private async Task StartAsync()
        {
            Logger?.Log("Starting FTP server", LogLevel.Debug);
            await _ftpServerService.StartAsync();
            IsServerRunning = true;
            Logger?.Log("FTP server started", LogLevel.Debug);
        }

        private async Task StopAsync()
        {
            Logger?.Log("Stopping FTP server", LogLevel.Debug);
            await _ftpServerService.StopAsync();
            IsServerRunning = false;
            Logger?.Log("FTP server stopped", LogLevel.Debug);
        }

        private void HandleFileReceived(object? sender, FtpTransferEventArgs e)
        {
            if (e == null)
                return;
            UploadedFiles.Add(e);
            Logger?.Log($"File uploaded: {e.Path}", LogLevel.Debug);
        }

        private void HandleFileSent(object? sender, FtpTransferEventArgs e)
        {
            if (e == null)
                return;
            DownloadedFiles.Add(e);
            Logger?.Log($"File downloaded: {e.Path}", LogLevel.Debug);
        }
    }
}

