using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Csv.UI.ViewModels.Csv.Advanced;

namespace DesktopApplicationTemplate.Services.Csv.UI.Views.Csv.Advanced;

public partial class CsvAdvancedConfigView : Page
{
    private readonly ILoggingService _logger;

    public CsvAdvancedConfigView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(CsvAdvancedConfigViewModel viewModel)
    {
        DataContext = viewModel;
        viewModel.Logger = _logger;
    }
}
