using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views;

public partial class MqttAdvancedConfigView : Page
{
    private readonly ILoggingService _logger;

    public MqttAdvancedConfigView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(MqttAdvancedConfigViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
