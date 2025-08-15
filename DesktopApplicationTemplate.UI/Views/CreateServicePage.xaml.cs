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
                ServiceCreated?.Invoke(name, meta.Type);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancelled?.Invoke();
        }
    }
}
