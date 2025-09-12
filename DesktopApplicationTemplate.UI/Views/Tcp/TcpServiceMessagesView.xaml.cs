using DesktopApplicationTemplate.UI.ViewModels;
using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels.Tcp;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Views.Tcp
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
            LogView.DataContext = new ServiceLogViewModel(service.Type, service.Logs);
            if (DataContext is TcpServiceMessagesViewModel vm)
                vm.SetService(service);
        }

    }
}
