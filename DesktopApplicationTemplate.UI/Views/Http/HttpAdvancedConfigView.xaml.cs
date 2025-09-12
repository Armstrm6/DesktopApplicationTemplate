using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Http.Advanced;

namespace DesktopApplicationTemplate.UI.Views.Http;

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
