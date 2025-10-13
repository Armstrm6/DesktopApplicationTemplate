using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class CreateServicePage : Page
    {
        private readonly CreateServiceViewModel _viewModel;
        public event Func<string, ServiceType, Task>? ServiceCreated;
        public event Action<ServiceType>? ServiceTypeSelected;
        public event Action? Cancelled;

        public CreateServicePage(CreateServiceViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        public void SetExistingNames(IEnumerable<string> names) => _viewModel.SetExistingNames(names);

        private void ServiceType_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { DataContext: CreateServiceViewModel.ServiceTypeMetadata meta })
            {
                return;
            }

            var name = _viewModel.GenerateDefaultName(meta.Type);
            if (meta.Type is ServiceType.Mqtt or ServiceType.Tcp or ServiceType.Heartbeat or ServiceType.Ftp or ServiceType.Http or ServiceType.Hid or ServiceType.Csv or ServiceType.FileObserver or ServiceType.Scp)
            {
                ServiceTypeSelected?.Invoke(meta.Type);
                return;
            }

            ObserveTask(OnServiceCreatedAsync(name, meta.Type));
        }

        public string GenerateDefaultName(ServiceType type) => _viewModel.GenerateDefaultName(type);

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancelled?.Invoke();
        }

        private Task OnServiceCreatedAsync(string name, ServiceType type)
        {
            return ServiceCreated?.Invoke(name, type) ?? Task.CompletedTask;
        }

        private static void ObserveTask(Task? task)
        {
            if (task is null)
            {
                return;
            }

            _ = task.ContinueWith(
                t => _ = t.Exception,
                System.Threading.CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }
    }
}
