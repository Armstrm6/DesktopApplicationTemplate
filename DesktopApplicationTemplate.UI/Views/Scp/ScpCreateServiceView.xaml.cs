using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Create;

namespace DesktopApplicationTemplate.UI.Views.Scp;

public partial class ScpCreateServiceView : Page
{
    public ScpCreateServiceView(ScpCreateServiceViewModel vm, ILoggingService logger)
    {
        InitializeComponent();
        DataContext = vm;
        vm.Logger = logger;
    }
}
