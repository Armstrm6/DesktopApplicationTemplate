using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.FileObserver;

namespace DesktopApplicationTemplate.UI.Views.FileObserver;

public partial class FileObserverAdvancedConfigView : Page
{
    private readonly ILoggingService _logger;

    public FileObserverAdvancedConfigView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(FileObserverAdvancedConfigViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
