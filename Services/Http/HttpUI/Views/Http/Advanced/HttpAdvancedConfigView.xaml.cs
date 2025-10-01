using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Http.UI.ViewModels.Http.Advanced;

namespace DesktopApplicationTemplate.Services.Http.UI.Views.Http.Advanced;

public partial class HttpAdvancedConfigView : Page
{
    private readonly ILoggingService _logger;

    public HttpAdvancedConfigView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(HttpAdvancedConfigViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
