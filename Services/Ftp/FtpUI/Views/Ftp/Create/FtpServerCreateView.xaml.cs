using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Ftp.UI.ViewModels.Ftp.Create;

namespace DesktopApplicationTemplate.Services.Ftp.UI.Views.Ftp.Create;

public partial class FtpServerCreateView : Page
{
    public FtpServerCreateView(FtpServerCreateViewModel vm, ILoggingService logger)
    {
        InitializeComponent();
        DataContext = vm;
        vm.Logger = logger;
    }
}
