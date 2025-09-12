using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Http.Create;

namespace DesktopApplicationTemplate.UI.Views.Http.Create;

public partial class HttpCreateServiceView : Page
{
    public HttpCreateServiceView(HttpCreateServiceViewModel vm, ILoggingService logger)
    {
        InitializeComponent();
        DataContext = vm;
        vm.Logger = logger;
    }
}
