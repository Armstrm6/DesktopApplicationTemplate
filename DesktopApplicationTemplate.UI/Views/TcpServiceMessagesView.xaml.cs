using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;

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

        public void SetServiceContext(ServiceViewModel service)
        {
            LogView.DataContext = new ServiceLogViewModel(service.DisplayName, service.ServiceType, service.Logs);
            if (DataContext is TcpServiceMessagesViewModel vm)
                vm.SetService(service);
        }

    }
}
