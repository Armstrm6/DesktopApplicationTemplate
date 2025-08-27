using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views;

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
