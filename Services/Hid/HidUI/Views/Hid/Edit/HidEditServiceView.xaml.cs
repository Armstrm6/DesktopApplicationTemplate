using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Hid.UI.ViewModels.Hid.Edit;

namespace DesktopApplicationTemplate.Services.Hid.UI.Views.Hid.Edit;

public partial class HidEditServiceView : Page
{
    private readonly ILoggingService _logger;

    public HidEditServiceView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(HidEditServiceViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
