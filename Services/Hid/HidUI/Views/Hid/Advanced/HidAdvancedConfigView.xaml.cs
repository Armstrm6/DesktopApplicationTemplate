using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Hid.UI.ViewModels.Hid.Advanced;

namespace DesktopApplicationTemplate.Services.Hid.UI.Views.Hid.Advanced;

public partial class HidAdvancedConfigView : Page
{
    private readonly ILoggingService _logger;

    public HidAdvancedConfigView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(HidAdvancedConfigViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
