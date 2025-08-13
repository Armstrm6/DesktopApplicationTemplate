using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.ViewModels
{
public class MqttServiceViewModel : ViewModelBase, ILoggingViewModel, INetworkAwareViewModel
    {
        private string _host = string.Empty;
        public string Host
        {
            get => _host;
            set
            {
                if (InputValidators.IsValidPartialIp(value))
                    _host = value;
                OnPropertyChanged();
            }
        }

        private string _port = "1883";
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

        private string _clientId = "client1";
        public string ClientId { get => _clientId; set { _clientId = value; OnPropertyChanged(); } }

        private string _username = string.Empty;
        public string Username { get => _username; set { _username = value; OnPropertyChanged(); } }

        private string _password = string.Empty;
        public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }

        private string _publishTopic = string.Empty;
        public string PublishTopic { get => _publishTopic; set { _publishTopic = value; OnPropertyChanged(); } }

        private string _publishMessage = string.Empty;
        public string PublishMessage { get => _publishMessage; set { _publishMessage = value; OnPropertyChanged(); } }

        private string _newTopic = string.Empty;
        public string NewTopic { get => _newTopic; set { _newTopic = value; OnPropertyChanged(); } }

        public ObservableCollection<string> Topics { get; } = new();

        public ICommand AddTopicCommand { get; }
        public ICommand RemoveTopicCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand PublishCommand { get; }
        public ICommand SaveCommand { get; }

        public ILoggingService? Logger { get; set; }

        private readonly MqttService _service;

        public MqttServiceViewModel(MqttService? service = null, ILoggingService? logger = null)
        {
            Logger = logger;
            _service = service ?? new MqttService(logger);
            AddTopicCommand = new RelayCommand(() => { if(!string.IsNullOrWhiteSpace(NewTopic)){Topics.Add(NewTopic); NewTopic = string.Empty;} });
            RemoveTopicCommand = new RelayCommand(() => { if(Topics.Contains(NewTopic)) Topics.Remove(NewTopic); });
            ConnectCommand = new RelayCommand(async () => await ConnectAsync());
            PublishCommand = new RelayCommand(async () => await PublishAsync());
            SaveCommand = new RelayCommand(Save);
        }

        public async Task ConnectAsync()
        {
            Logger?.Log("MQTT connect start", LogLevel.Debug);
            await _service.ConnectAsync(Host, int.Parse(Port), ClientId, Username, Password);
            await _service.SubscribeAsync(Topics);
            Logger?.Log("MQTT connected", LogLevel.Debug);
            Logger?.Log("MQTT connect finished", LogLevel.Debug);
        }

        public async Task PublishAsync()
        {
            if(string.IsNullOrWhiteSpace(PublishTopic) || string.IsNullOrWhiteSpace(PublishMessage))
                return;
            Logger?.Log("MQTT publish start", LogLevel.Debug);
            await _service.PublishAsync(PublishTopic, PublishMessage);
            Logger?.Log($"Published to {PublishTopic}", LogLevel.Debug);
            Logger?.Log("MQTT publish finished", LogLevel.Debug);
        }

        private void Save() => SaveConfirmationHelper.Show();

        public void UpdateNetworkConfiguration(NetworkConfiguration configuration)
        {
            Host = configuration.IpAddress;
        }

        // OnPropertyChanged provided by ViewModelBase
    }
}
