using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class ScpServiceViewModel : ViewModelBase
    {
        private string _host = string.Empty;
        public string Host { get => _host; set { _host = value; OnPropertyChanged(); } }

        private string _port = "22";
        public string Port { get => _port; set { _port = value; OnPropertyChanged(); } }

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

        public ScpServiceViewModel()
        {
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
            var svc = new ScpService(Host, int.Parse(Port), Username, Password);
            await svc.UploadAsync(LocalPath, RemotePath);
            Logger?.Log("File transferred", LogLevel.Debug);
        }

        private void Save() => System.Windows.MessageBox.Show("Configuration saved.");

        // OnPropertyChanged provided by ViewModelBase
    }
}
