using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Tcp.Create;

namespace DesktopApplicationTemplate.UI.Views.Tcp;

public partial class TcpCreateServiceView : Page
{
    public TcpCreateServiceView(TcpCreateServiceViewModel vm, ILoggingService logger)
    {
        InitializeComponent();
        DataContext = vm;
        vm.Logger = logger;
    }
}
