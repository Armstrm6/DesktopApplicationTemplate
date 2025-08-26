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
        public event Action<string>? MqttSelected;
        public event Action<string>? TcpSelected;
        public event Action<string>? FtpServerSelected;
        public event Action<string>? HttpSelected;
        public event Action<string>? HidSelected;
        public event Action<string>? FileObserverSelected;
        public event Action? Cancelled;

        public CreateServicePage(CreateServiceViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void ServiceType_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CreateServiceViewModel.ServiceTypeMetadata meta)
            {
                var name = _viewModel.GenerateDefaultName(meta.Type);
                if (meta.Type == "MQTT")
                {
                    MqttSelected?.Invoke(name);
                    return;
                }
                if (meta.Type == "TCP")
                {
                    TcpSelected?.Invoke(name);
                    return;
                }
                if (meta.Type == "FTP" || meta.Type == "FTP Server")
                {
                    FtpServerSelected?.Invoke(name);
                    return;
                }
                if (meta.Type == "HTTP")
                {
                    HttpSelected?.Invoke(name);
                    return;
                }
                if (meta.Type == "HID")
                {
                    HidSelected?.Invoke(name);
                    return;
                }
                if (meta.Type == "File Observer")
                {
                    FileObserverSelected?.Invoke(name);
                    return;
                }
                ServiceCreated?.Invoke(name, meta.Type);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancelled?.Invoke();
        }
    }
}
