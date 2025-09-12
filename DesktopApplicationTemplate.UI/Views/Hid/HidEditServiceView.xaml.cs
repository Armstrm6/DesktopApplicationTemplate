using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Hid.Edit;

namespace DesktopApplicationTemplate.UI.Views.Hid;

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
