using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class TcpServiceViewModel : INotifyPropertyChanged
    {
        private string _statusMessage = string.Empty;
        private bool _isServerRunning;

        private string _computerIp = string.Empty;
        private string _listeningPort = string.Empty;
        private string _serverIp = string.Empty;
        private string _serverGateway = string.Empty;
        private string _serverPort = string.Empty;

        private string _scriptContent = string.Empty;
        private string _selectedLanguage = "Python";
        private string _testMessage = string.Empty;

        public ObservableCollection<string> ScriptLanguages { get; } = new() { "Python", "C#" };

        public ILoggingService? Logger { get; set; }

        public string ComputerIp
        {
            get => _computerIp;
            set { _computerIp = value; OnPropertyChanged(); }
        }

        public string ListeningPort
        {
            get => _listeningPort;
            set { _listeningPort = value; OnPropertyChanged(); }
        }

        public string ServerIp
        {
            get => _serverIp;
            set { _serverIp = value; OnPropertyChanged(); }
        }

        public string ServerGateway
        {
            get => _serverGateway;
            set { _serverGateway = value; OnPropertyChanged(); }
        }

        public string ServerPort
        {
            get => _serverPort;
            set { _serverPort = value; OnPropertyChanged(); }
        }

        public string ScriptContent
        {
            get => _scriptContent;
            set { _scriptContent = value; OnPropertyChanged(); }
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set { _selectedLanguage = value; OnPropertyChanged(); SetDefaultTemplate(); }
        }

        public string TestMessage
        {
            get => _testMessage;
            set { _testMessage = value; OnPropertyChanged(); }
        }

        public ICommand ToggleServerCommand { get; }
        public ICommand TestScriptCommand { get; }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsServerRunning
        {
            get => _isServerRunning;
            set
            {
                _isServerRunning = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }

        public TcpServiceViewModel()
        {
            StatusMessage = "Chappie is initializing...";
            IsServerRunning = false;
            SaveCommand = new RelayCommand(Save);
            ToggleServerCommand = new RelayCommand(ToggleServer);
            TestScriptCommand = new RelayCommand(TestScript);
            SetDefaultTemplate();
        }

        private void SetDefaultTemplate()
        {
            ScriptContent = SelectedLanguage == "Python"
                ? "def process(message):\n    # message is the incoming string\n    return message"
                : "string Process(string message)\n{\n    // message is the incoming string\n    return message;\n}";
        }

        private void ToggleServer()
        {
            IsServerRunning = !IsServerRunning;
            Logger?.Log(IsServerRunning ? "Server Started" : "Server Stopped", LogLevel.Debug);
        }

        private void TestScript()
        {
            var result = ScriptContent.Replace("message", TestMessage);
            Logger?.Log($"Script output: {result}", LogLevel.Debug);
            MessageBox.Show(result, "Test Result");
        }

        private void Save()
        {
            MessageBox.Show("Configuration saved.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
