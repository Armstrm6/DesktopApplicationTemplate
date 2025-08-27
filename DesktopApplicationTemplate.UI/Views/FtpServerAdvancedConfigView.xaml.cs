using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views;

public partial class FtpServerAdvancedConfigView : Page
{
    private readonly ILoggingService _logger;

    public FtpServerAdvancedConfigView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(FtpServerAdvancedConfigViewModel vm)
    {
        if (vm is null)
            throw new ArgumentNullException(nameof(vm));

        DataContext = vm;
        vm.Logger = _logger;
    }
}
