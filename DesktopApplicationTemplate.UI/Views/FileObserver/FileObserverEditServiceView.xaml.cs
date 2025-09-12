using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.FileObserver;

namespace DesktopApplicationTemplate.UI.Views.FileObserver;

public partial class FileObserverEditServiceView : Page
{
    private readonly ILoggingService _logger;

    public FileObserverEditServiceView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(FileObserverEditServiceViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
