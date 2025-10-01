using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Hid.UI.ViewModels.Hid.Create;

namespace DesktopApplicationTemplate.Services.Hid.UI.Views.Hid.Create;

public partial class HidCreateServiceView : Page
{
    public HidCreateServiceView(HidCreateServiceViewModel vm, ILoggingService logger)
    {
        InitializeComponent();
        DataContext = vm;
        vm.Logger = logger;
    }
}
