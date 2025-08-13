using System;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;

namespace DesktopApplicationTemplate.UI.ViewModels
{
public class TcpServiceViewModel : ValidatableViewModelBase, ILoggingViewModel, INetworkAwareViewModel
    {
        private string _statusMessage = string.Empty;
        private bool _isServerRunning;

        private string _computerIp = string.Empty;
        private string _listeningPort = string.Empty;
        private string _serverIp = string.Empty;
        private string _serverGateway = string.Empty;
        private string _serverPort = string.Empty;

        private bool _isUdp;
        private bool _serverIpEnabled = true;
        private string _selectedMode = "Listening";

        public bool IsUdp
        {
            get => _isUdp;
            set
            {
                _isUdp = value;
                if (_isUdp)
                {
                    ServerIp = "0.0.0.0";
                    ServerIpEnabled = false;
                }
                else
                {
                    ServerIpEnabled = true;
                }
                OnPropertyChanged();
            }
        }

        public bool ServerIpEnabled
        {
            get => _serverIpEnabled;
            set { _serverIpEnabled = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Modes { get; } = new() { "Listening", "Sending" };

        public string SelectedMode
        {
            get => _selectedMode;
            set { _selectedMode = value; OnPropertyChanged(); }
        }

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
                _computerIp = value;
                if (!InputValidators.IsValidPartialIp(value))
                {
                    AddError(nameof(ComputerIp), "Invalid IP address");
                    Logger?.Log("Invalid computer IP entered", LogLevel.Warning);
                }
                else
                {
                    ClearErrors(nameof(ComputerIp));
                }
                OnPropertyChanged();
            }
        }

        public string ListeningPort
        {
            get => _listeningPort;
            set
            {
                _listeningPort = value;
                if (!int.TryParse(value, out _))
                {
                    AddError(nameof(ListeningPort), "Port must be numeric");
                    Logger?.Log("Invalid listening port entered", LogLevel.Warning);
                }
                else
                {
                    ClearErrors(nameof(ListeningPort));
                }
                OnPropertyChanged();
            }
        }

        public string ServerIp
        {
            get => _serverIp;
            set
            {
                _serverIp = value;
                if (!InputValidators.IsValidPartialIp(value))
                {
                    AddError(nameof(ServerIp), "Invalid IP address");
                    Logger?.Log("Invalid server IP entered", LogLevel.Warning);
                }
                else
                {
                    ClearErrors(nameof(ServerIp));
                }
                OnPropertyChanged();
            }
        }

        public string ServerGateway
        {
            get => _serverGateway;
            set
            {
                _serverGateway = value;
                if (!InputValidators.IsValidPartialIp(value))
                {
                    AddError(nameof(ServerGateway), "Invalid IP address");
                    Logger?.Log("Invalid server gateway entered", LogLevel.Warning);
                }
                else
                {
                    ClearErrors(nameof(ServerGateway));
                }
                OnPropertyChanged();
            }
        }

        public string ServerPort
        {
            get => _serverPort;
            set
            {
                _serverPort = value;
                if (!int.TryParse(value, out _))
                {
                    AddError(nameof(ServerPort), "Port must be numeric");
                    Logger?.Log("Invalid server port entered", LogLevel.Warning);
                }
                else
                {
                    ClearErrors(nameof(ServerPort));
                }
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

        private readonly SaveConfirmationHelper _saveHelper;

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

        public TcpServiceViewModel(SaveConfirmationHelper saveHelper)
        {
            _saveHelper = saveHelper;
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

        private void Save() => _saveHelper.Show();

        public void UpdateNetworkConfiguration(NetworkConfiguration configuration)
        {
            ComputerIp = configuration.IpAddress;
            ServerGateway = configuration.Gateway;
        }

        // OnPropertyChanged inherited from ViewModelBase
    }
}
