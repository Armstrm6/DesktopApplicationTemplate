using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Tcp.Edit;

namespace DesktopApplicationTemplate.UI.Views.Tcp.Edit;

public partial class TcpEditServiceView : Page
{
    private readonly ILoggingService _logger;

    public TcpEditServiceView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(TcpEditServiceViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
