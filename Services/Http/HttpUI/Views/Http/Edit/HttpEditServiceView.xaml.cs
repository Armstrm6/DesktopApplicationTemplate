using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Http.UI.ViewModels.Http.Edit;

namespace DesktopApplicationTemplate.Services.Http.UI.Views.Http.Edit;

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
