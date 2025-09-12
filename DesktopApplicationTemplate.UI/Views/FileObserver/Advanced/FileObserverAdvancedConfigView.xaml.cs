using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.FileObserver.Advanced;

namespace DesktopApplicationTemplate.UI.Views.FileObserver.Advanced;

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
