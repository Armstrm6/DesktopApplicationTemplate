using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.ViewModels
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

        public ILoggingService? Logger { get; set; }

        private readonly SaveConfirmationHelper _saveHelper;

        public ScpServiceViewModel(SaveConfirmationHelper saveHelper)
        {
            _saveHelper = saveHelper;
            BrowseCommand = new RelayCommand(Browse);
            TransferCommand = new RelayCommand(async () => await TransferAsync());
            SaveCommand = new RelayCommand(Save);
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
            var svc = new ScpService(Host, int.Parse(Port), Username, Password, Logger);
            await svc.UploadAsync(LocalPath, RemotePath);
            Logger?.Log("File transferred", LogLevel.Debug);
            Logger?.Log("SCP transfer finished", LogLevel.Debug);
        }

        private void Save() => _saveHelper.Show();

        public void UpdateNetworkConfiguration(NetworkConfiguration configuration)
        {
            Host = configuration.IpAddress;
        }

        // OnPropertyChanged provided by ViewModelBase
    }
}
