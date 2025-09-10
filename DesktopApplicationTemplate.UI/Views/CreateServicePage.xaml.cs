using System;
using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class CreateServicePage : Page
    {
        private readonly CreateServiceViewModel _viewModel;
        public event Action<string, string>? ServiceCreated;
        public event Action<string>? ServiceTypeSelected;
        public event Action? Cancelled;

        public CreateServicePage(CreateServiceViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void ServiceType_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: CreateServiceViewModel.ServiceTypeMetadata meta } button)
            {
                var name = _viewModel.GenerateDefaultName(meta.Type);
                if (meta.Type is "MQTT" or "TCP" or "Heartbeat" or "FTP" or "FTP Server" or "HTTP" or "HID" or "CSV Creator" or "File Observer" or "SCP")
                {
                    ServiceTypeSelected?.Invoke(meta.Type);
                    return;
                }
                ServiceCreated?.Invoke(name, meta.Type);
            }
        }

        public string GenerateDefaultName(string type) => _viewModel.GenerateDefaultName(type);

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancelled?.Invoke();
        }
    }
}
