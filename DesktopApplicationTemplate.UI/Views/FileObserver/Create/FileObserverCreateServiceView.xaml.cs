using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.FileObserver.Create;

namespace DesktopApplicationTemplate.UI.Views.FileObserver.Create;

public partial class FileObserverCreateServiceView : Page
{
    public FileObserverCreateServiceView(FileObserverCreateServiceViewModel vm, ILoggingService logger)
    {
        InitializeComponent();
        DataContext = vm;
        vm.Logger = logger;
    }
}
