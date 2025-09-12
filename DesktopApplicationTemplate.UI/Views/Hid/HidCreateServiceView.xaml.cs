using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Hid;

namespace DesktopApplicationTemplate.UI.Views.Hid;

public partial class HidCreateServiceView : Page
{
    public HidCreateServiceView(HidCreateServiceViewModel vm, ILoggingService logger)
    {
        InitializeComponent();
        DataContext = vm;
        vm.Logger = logger;
    }
}
