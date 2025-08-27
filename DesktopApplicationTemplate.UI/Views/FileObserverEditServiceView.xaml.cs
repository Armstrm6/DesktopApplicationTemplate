using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views;

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
