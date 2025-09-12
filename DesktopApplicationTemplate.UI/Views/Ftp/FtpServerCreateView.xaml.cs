using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Ftp;

namespace DesktopApplicationTemplate.UI.Views.Ftp;

public partial class FtpServerCreateView : Page
{
    public FtpServerCreateView(FtpServerCreateViewModel vm, ILoggingService logger)
    {
        InitializeComponent();
        DataContext = vm;
        vm.Logger = logger;
    }
}
