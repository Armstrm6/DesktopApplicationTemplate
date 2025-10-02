using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views;

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
