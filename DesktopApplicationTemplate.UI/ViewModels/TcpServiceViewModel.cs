using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace DesktopApplicationTemplate.UI.ViewModels
{
public class TcpServiceViewModel : ValidatableViewModelBase, ILoggingViewModel, INetworkAwareViewModel
    {
        private readonly TcpServiceMessagesViewModel _messagesViewModel;

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
                ServerIpEnabled = !_isUdp;
                OnPropertyChanged();
                UpdateMessageViewModelNetworkSettings();
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
        private string _inputMessage = string.Empty;
        private string _outputMessage = string.Empty;

        private TcpServiceOptions _options = new();

        public ObservableCollection<string> ScriptLanguages { get; } = new() { "Python", "C#" };

        public ILoggingService? Logger { get; set; }

        public string ComputerIp
        {
            get => _computerIp;
            set
            {
                _computerIp = value;
                if (!InputValidators.IsValidHost(value))
                {
                    AddError(nameof(ComputerIp), "Invalid IP address");
                    Logger?.Log("Invalid computer IP entered", LogLevel.Warning);
                }
                else
                {
                    ClearErrors(nameof(ComputerIp));
                }
                OnPropertyChanged();
                UpdateMessageViewModelNetworkSettings();
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
                UpdateMessageViewModelNetworkSettings();
            }
        }

        public string ServerIp
        {
            get => _serverIp;
            set
            {
                _serverIp = value;
                if (!InputValidators.IsValidHost(value))
                {
                    AddError(nameof(ServerIp), "Invalid IP address");
                    Logger?.Log("Invalid server IP entered", LogLevel.Warning);
                }
                else
                {
                    ClearErrors(nameof(ServerIp));
                }
                OnPropertyChanged();
                UpdateMessageViewModelNetworkSettings();
            }
        }

        public string ServerGateway
        {
            get => _serverGateway;
            set
            {
                _serverGateway = value;
                if (!InputValidators.IsValidHost(value))
                {
                    AddError(nameof(ServerGateway), "Invalid IP address");
                    Logger?.Log("Invalid server gateway entered", LogLevel.Warning);
                }
                else
                {
                    ClearErrors(nameof(ServerGateway));
                }
                OnPropertyChanged();
                UpdateMessageViewModelNetworkSettings();
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
                UpdateMessageViewModelNetworkSettings();
            }
        }

        public string ScriptContent
        {
            get => _scriptContent;
            set
            {
                _scriptContent = value;
                OnPropertyChanged();
                _messagesViewModel.UpdateScript(_scriptContent);
            }
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set { _selectedLanguage = value; OnPropertyChanged(); SetDefaultTemplate(); }
        }

        public string InputMessage
        {
            get => _inputMessage;
            set
            {
                _inputMessage = value;
                OnPropertyChanged();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (SettingsViewModel.TcpLoggingEnabled)
                        Logger?.Log($"Received test message: {value}", LogLevel.Debug);
                }
            }
        }

        public string OutputMessage
        {
            get => _outputMessage;
            set { _outputMessage = value; OnPropertyChanged(); }
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

        /// <summary>
        /// Command to navigate back without saving.
        /// </summary>
        public ICommand BackCommand { get; }

        public TcpServiceViewModel(SaveConfirmationHelper saveHelper, TcpServiceMessagesViewModel messagesViewModel)
        {
            _saveHelper = saveHelper;
            _messagesViewModel = messagesViewModel ?? throw new ArgumentNullException(nameof(messagesViewModel));
            StatusMessage = "Chappie is initializing...";
            IsServerRunning = false;
            SaveCommand = new RelayCommand(Save);
            BackCommand = new RelayCommand(Back);
            ToggleServerCommand = new RelayCommand(ToggleServer);
            TestScriptCommand = new AsyncRelayCommand(TestScriptAsync);
            SetDefaultTemplate();
            UpdateMessageViewModelNetworkSettings();
        }

        public void Load(TcpServiceOptions options)
        {
            _options = options ?? new TcpServiceOptions();
            ComputerIp = options.Host;
            ListeningPort = options.Port.ToString();
            IsUdp = options.UseUdp;
            SelectedMode = options.Mode.ToString();
            ScriptContent = options.Script;
            InputMessage = options.InputMessage;
            OutputMessage = options.OutputMessage;
            _messagesViewModel.UpdateScript(ScriptContent);
        }

        private void SetDefaultTemplate()
        {
            ScriptContent = SelectedLanguage == "Python"
                ? "def process(message):\n    # message is the incoming string\n    return message"
                : "string Process(string message)\n{\n    // message is the incoming string\n    return message;\n}";
            _messagesViewModel.UpdateScript(ScriptContent);
        }

        private void ToggleServer()
        {
            if (SettingsViewModel.TcpLoggingEnabled)
                Logger?.Log("Toggling server state", LogLevel.Debug);

            IsServerRunning = !IsServerRunning;

            if (!SettingsViewModel.TcpLoggingEnabled)
                return;

            var message = IsServerRunning
                ? $"Server started on {ComputerIp}:{ListeningPort}"
                : "Server stopped";
            Logger?.Log(message, LogLevel.Debug);
        }

        private async Task TestScriptAsync()
        {
            if (string.IsNullOrWhiteSpace(InputMessage))
            {
                if (SettingsViewModel.TcpLoggingEnabled)
                    Logger?.Log("TestScript called with empty message", LogLevel.Warning);
                return;
            }

            if (SettingsViewModel.TcpLoggingEnabled)
                Logger?.Log($"Executing script using {SelectedLanguage}", LogLevel.Debug);

            try
            {
                if (SelectedLanguage == "C#")
                {
                    var script = $"{ScriptContent}\nProcess(\"{InputMessage}\");";
                    var result = await CSharpScript.EvaluateAsync<string>(script);
                    if (SettingsViewModel.TcpLoggingEnabled)
                        Logger?.Log($"Script output: {result}", LogLevel.Debug);
                    OutputMessage = result;
                    MessageBox.Show(result, "Test Result");
                }
                else if (SettingsViewModel.TcpLoggingEnabled)
                {
                    Logger?.Log("Python execution not supported", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                if (SettingsViewModel.TcpLoggingEnabled)
                    Logger?.Log($"Script execution error: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Raised when the configuration is saved.
        /// </summary>
        public event EventHandler? Saved;

        /// <summary>
        /// Raised when navigation back is requested without saving.
        /// </summary>
        public event EventHandler? BackRequested;

        public event EventHandler? RequestClose;

        private void Save()
        {
            _options.Host = ComputerIp;
            _options.Port = int.TryParse(ListeningPort, out var p) ? p : 0;
            _options.UseUdp = IsUdp;
            _options.Mode = Enum.TryParse<TcpServiceMode>(SelectedMode, out var m) ? m : TcpServiceMode.Listening;
            _options.Script = ScriptContent;
            _options.InputMessage = InputMessage;
            _options.OutputMessage = OutputMessage;
            _saveHelper.Show();
            Saved?.Invoke(this, EventArgs.Empty);
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void Back()
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateNetworkConfiguration(NetworkConfiguration configuration)
        {
            ComputerIp = configuration.IpAddress;
            ServerGateway = configuration.Gateway;
            UpdateMessageViewModelNetworkSettings();
        }

        private void UpdateMessageViewModelNetworkSettings()
        {
            _messagesViewModel.UpdateNetworkSettings(ComputerIp, ListeningPort, ServerIp, ServerGateway, ServerPort, IsUdp);
        }

        // OnPropertyChanged inherited from ViewModelBase
    }
}
