using System;
using Microsoft.CodeAnalysis.CSharp.Scripting;
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
    public class TcpServiceViewModel : ViewModelBase
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
            set
            {
                if (System.Net.IPAddress.TryParse(value, out _) || string.IsNullOrWhiteSpace(value))
                    _computerIp = value;
                OnPropertyChanged();
            }
        }

        public string ListeningPort
        {
            get => _listeningPort;
            set
            {
                if (int.TryParse(value, out _))
                    _listeningPort = value;
                OnPropertyChanged();
            }
        }

        public string ServerIp
        {
            get => _serverIp;
            set
            {
                if (System.Net.IPAddress.TryParse(value, out _) || string.IsNullOrWhiteSpace(value))
                    _serverIp = value;
                OnPropertyChanged();
            }
        }

        public string ServerGateway
        {
            get => _serverGateway;
            set
            {
                if (System.Net.IPAddress.TryParse(value, out _) || string.IsNullOrWhiteSpace(value))
                    _serverGateway = value;
                OnPropertyChanged();
            }
        }

        public string ServerPort
        {
            get => _serverPort;
            set
            {
                if (int.TryParse(value, out _))
                    _serverPort = value;
                OnPropertyChanged();
            }
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
            set
            {
                _testMessage = value;
                OnPropertyChanged();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (SettingsViewModel.TcpLoggingEnabled)
                        Logger?.Log($"Received test message: {value}", LogLevel.Debug);
                }
            }
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
            if (SettingsViewModel.TcpLoggingEnabled)
                Logger?.Log("Toggling server state", LogLevel.Debug);
            IsServerRunning = !IsServerRunning;
            if (IsServerRunning)
                if (SettingsViewModel.TcpLoggingEnabled)
                    Logger?.Log($"Server started on {ComputerIp}:{ListeningPort}", LogLevel.Debug);
            else
                if (SettingsViewModel.TcpLoggingEnabled)
                    Logger?.Log("Server stopped", LogLevel.Debug);
        }

        private void TestScript()
        {
            if (string.IsNullOrWhiteSpace(TestMessage))
            {
                if (SettingsViewModel.TcpLoggingEnabled)
                    Logger?.Log("TestScript called with empty message", LogLevel.Warning);
            }
            if (SettingsViewModel.TcpLoggingEnabled)
                Logger?.Log($"Executing script using {SelectedLanguage}", LogLevel.Debug);
            try
            {
                if (SelectedLanguage == "C#")
                {
                    var script = ScriptContent + $"\nProcess(\"{TestMessage}\");";
                    var result = CSharpScript.EvaluateAsync<string>(script).Result;
                    if (SettingsViewModel.TcpLoggingEnabled)
                        Logger?.Log($"Script output: {result}", LogLevel.Debug);
                    System.Windows.MessageBox.Show(result, "Test Result");
                }
                else
                {
                    if (SettingsViewModel.TcpLoggingEnabled)
                        Logger?.Log("Python execution not supported", LogLevel.Warning);
                }
            }
            catch (System.Exception ex)
            {
                if (SettingsViewModel.TcpLoggingEnabled)
                    Logger?.Log($"Script execution error: {ex.Message}", LogLevel.Error);
            }
        }

        private void Save()
        {
            System.Windows.MessageBox.Show("Configuration saved.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // OnPropertyChanged inherited from ViewModelBase
    }
}
