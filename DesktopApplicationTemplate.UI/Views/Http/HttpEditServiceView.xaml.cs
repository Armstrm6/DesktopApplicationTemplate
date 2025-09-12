using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Http;

namespace DesktopApplicationTemplate.UI.Views.Http;

public partial class HttpEditServiceView : Page
{
    private readonly ILoggingService _logger;

    public HttpEditServiceView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(HttpEditServiceViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
