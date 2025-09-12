using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Edit;

namespace DesktopApplicationTemplate.UI.Views.Scp;

public partial class ScpEditServiceView : Page
{
    private readonly ILoggingService _logger;

    public ScpEditServiceView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(ScpEditServiceViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
