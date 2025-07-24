using System.Runtime.CompilerServices;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;

namespace DesktopApplicationTemplate.UI.ViewModels
{
public class FtpServiceViewModel : ValidatableViewModelBase, ILoggingViewModel
    {
        private string _host = string.Empty;
        public string Host
        {
            get => _host;
            set
            {
                _host = value;
                if (!InputValidators.IsValidPartialIp(value))
                {
                    AddError(nameof(Host), "Invalid host or IP address");
                    Logger?.Log("Invalid FTP host entered", LogLevel.Warning);
                }
                else
                {
                    ClearErrors(nameof(Host));
                }
                OnPropertyChanged();
            }
        }

        private string _port = "21";
        public string Port
        {
            get => _port;
            set
            {
                _port = value;
                if (!int.TryParse(value, out _))
                {
                    AddError(nameof(Port), "Port must be numeric");
                    Logger?.Log("Invalid FTP port entered", LogLevel.Warning);
                }
                else
                {
                    ClearErrors(nameof(Port));
                }
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

        public ILoggingService? Logger { get; set; }

        /// <summary>
        /// Optional service used for testing to avoid real network operations.
        /// </summary>
        public IFtpService? Service { get; set; }

        private readonly IFileDialogService _fileDialog;

        public FtpServiceViewModel(IFileDialogService? fileDialog = null)
        {
            _fileDialog = fileDialog ?? new FileDialogService();
            BrowseCommand = new RelayCommand(Browse);
            TransferCommand = new RelayCommand(async () => await TransferAsync());
            SaveCommand = new RelayCommand(Save);
        }

        private void Browse()
        {
            Logger?.Log("Browsing for file", LogLevel.Debug);
            var path = _fileDialog.OpenFile();
            if (path != null)
            {
                LocalPath = path;
                Logger?.Log($"File selected: {path}", LogLevel.Debug);
            }
        }

        internal async Task TransferAsync()
        {
            if (string.IsNullOrWhiteSpace(LocalPath) || string.IsNullOrWhiteSpace(RemotePath))
                return;
            Logger?.Log("Starting FTP upload", LogLevel.Debug);
            var svc = Service ?? (IFtpService)new FtpService(Host, int.Parse(Port), Username, Password, Logger);
            await svc.UploadAsync(LocalPath, RemotePath);
            Logger?.Log("FTP upload complete", LogLevel.Debug);
        }

        private void Save() => SaveConfirmationHelper.Show();

        // OnPropertyChanged provided by ViewModelBase
    }
}
