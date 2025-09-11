using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Views
{
    /// <summary>
    /// Interaction logic for TcpServiceMessagesView.xaml
    /// </summary>
    public partial class TcpServiceMessagesView : Page, IServiceLogHost
    {
        public TcpServiceMessagesView(TcpServiceMessagesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public void SetServiceContext(ServiceListModel service)
        {
            Enum.TryParse<ServiceType>(service.ServiceType, true, out var type);
            LogView.DataContext = new ServiceLogViewModel(type, service.Logs);
            if (DataContext is TcpServiceMessagesViewModel vm)
                vm.SetService(service);
        }

    }
}
