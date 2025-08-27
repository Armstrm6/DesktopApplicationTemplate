using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views;

public partial class MqttEditServiceView : Page
{
    private readonly ILoggingService _logger;

    public MqttEditServiceView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(MqttEditServiceViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
