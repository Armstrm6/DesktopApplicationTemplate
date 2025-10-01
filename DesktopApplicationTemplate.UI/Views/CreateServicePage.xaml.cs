using System;
using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Navigation;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class CreateServicePage : Page
    {
        private readonly CreateServiceViewModel _viewModel;
        private readonly IServiceUiRegistry _uiRegistry;
        public event Action<string, string>? ServiceCreated;
        public event Action<string>? ServiceDescriptorSelected;
        public event Action? Cancelled;

        public CreateServicePage(CreateServiceViewModel viewModel, IServiceUiRegistry uiRegistry)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _uiRegistry = uiRegistry;
            DataContext = _viewModel;
        }

        private void ServiceType_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: CreateServiceViewModel.ServiceDescriptorMetadata meta })
            {
                var descriptorId = meta.DescriptorId;
                var name = _viewModel.GenerateDefaultName(descriptorId);
                if (_uiRegistry.NavigationHandlers.ContainsKey(descriptorId))
                {
                    ServiceDescriptorSelected?.Invoke(descriptorId);
                    return;
                }
                ServiceCreated?.Invoke(name, descriptorId);
            }
        }

        public string GenerateDefaultName(string descriptorId) => _viewModel.GenerateDefaultName(descriptorId);

        public bool TryGetLegacyType(string descriptorId, out ServiceType serviceType) => _viewModel.TryGetLegacyType(descriptorId, out serviceType);

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancelled?.Invoke();
        }
    }
}
