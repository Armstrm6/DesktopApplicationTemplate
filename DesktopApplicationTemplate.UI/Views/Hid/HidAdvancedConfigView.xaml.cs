using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Hid.Advanced;

namespace DesktopApplicationTemplate.UI.Views.Hid;

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
