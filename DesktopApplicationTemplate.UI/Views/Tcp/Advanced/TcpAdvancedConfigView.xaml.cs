using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Tcp.Advanced;

namespace DesktopApplicationTemplate.UI.Views.Tcp.Advanced;

public partial class TcpAdvancedConfigView : Page
{
    private readonly ILoggingService _logger;

    public TcpAdvancedConfigView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(TcpAdvancedConfigViewModel viewModel)
    {
        DataContext = viewModel;
        viewModel.Logger = _logger;
    }
}
