using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;

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
            ApplyCommand = new AsyncRelayCommand(ApplyAsync);
            RefreshCommand = new AsyncRelayCommand(LoadAsync);
        }

        public ObservableCollection<string> Interfaces { get; } = new();

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

        private string _selectedInterface = string.Empty;
        public string SelectedInterface { get => _selectedInterface; set { _selectedInterface = value; OnPropertyChanged(); } }

        public NetworkConfiguration CurrentConfiguration { get; private set; } = new();

        public ICommand ApplyCommand { get; }
        public ICommand RefreshCommand { get; }

        public async Task LoadAsync()
        {
            var available = await _service.GetAvailableInterfacesAsync().ConfigureAwait(false);
            Interfaces.Clear();
            foreach (var adapter in available)
            {
                Interfaces.Add(adapter);
            }

            CurrentConfiguration = await _service.GetConfigurationAsync().ConfigureAwait(false);
            IpAddress = CurrentConfiguration.IpAddress;
            SubnetMask = CurrentConfiguration.SubnetMask;
            Gateway = CurrentConfiguration.Gateway;
            DnsPrimary = CurrentConfiguration.DnsPrimary;
            DnsSecondary = CurrentConfiguration.DnsSecondary;
            if (!string.IsNullOrWhiteSpace(CurrentConfiguration.InterfaceName) &&
                !Interfaces.Any(adapter => string.Equals(adapter, CurrentConfiguration.InterfaceName, StringComparison.OrdinalIgnoreCase)))
            {
                Interfaces.Add(CurrentConfiguration.InterfaceName);
            }
            SelectedInterface = !string.IsNullOrWhiteSpace(CurrentConfiguration.InterfaceName)
                ? CurrentConfiguration.InterfaceName
                : Interfaces.FirstOrDefault() ?? string.Empty;
            _logger?.Log("Loaded network configuration", LogLevel.Debug);
        }

        public async Task ApplyAsync()
        {
            var config = new NetworkConfiguration
            {
                InterfaceName = SelectedInterface,
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
