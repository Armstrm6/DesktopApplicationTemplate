using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Helpers;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class NetworkConfigurationViewModel : ViewModelBase
    {
        private readonly INetworkConfigurationService _service;
        private readonly ILoggingService? _logger;

        public NetworkConfigurationViewModel(INetworkConfigurationService service, ILoggingService? logger = null)
        {
            _service = service;
            _logger = logger;
            ApplyCommand = new RelayCommand(async () => await ApplyAsync());
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
        }

        private string _ipAddress = string.Empty;
        public string IpAddress { get => _ipAddress; set { _ipAddress = value; OnPropertyChanged(); } }

        private string _subnetMask = string.Empty;
        public string SubnetMask { get => _subnetMask; set { _subnetMask = value; OnPropertyChanged(); } }

        private string _gateway = string.Empty;
        public string Gateway { get => _gateway; set { _gateway = value; OnPropertyChanged(); } }

        private string _dnsPrimary = string.Empty;
        public string DnsPrimary { get => _dnsPrimary; set { _dnsPrimary = value; OnPropertyChanged(); } }

        private string _dnsSecondary = string.Empty;
        public string DnsSecondary { get => _dnsSecondary; set { _dnsSecondary = value; OnPropertyChanged(); } }

        public NetworkConfiguration CurrentConfiguration { get; private set; } = new();

        public ICommand ApplyCommand { get; }
        public ICommand RefreshCommand { get; }

        public async Task LoadAsync()
        {
            CurrentConfiguration = await _service.GetConfigurationAsync().ConfigureAwait(false);
            IpAddress = CurrentConfiguration.IpAddress;
            SubnetMask = CurrentConfiguration.SubnetMask;
            Gateway = CurrentConfiguration.Gateway;
            DnsPrimary = CurrentConfiguration.DnsPrimary;
            DnsSecondary = CurrentConfiguration.DnsSecondary;
            _logger?.Log("Loaded network configuration", LogLevel.Debug);
        }

        public async Task ApplyAsync()
        {
            var config = new NetworkConfiguration
            {
                IpAddress = IpAddress,
                SubnetMask = SubnetMask,
                Gateway = Gateway,
                DnsPrimary = DnsPrimary,
                DnsSecondary = DnsSecondary
            };
            await _service.ApplyConfigurationAsync(config).ConfigureAwait(false);
            CurrentConfiguration = config;
            _logger?.Log("Applied network configuration", LogLevel.Debug);
        }
    }
}
