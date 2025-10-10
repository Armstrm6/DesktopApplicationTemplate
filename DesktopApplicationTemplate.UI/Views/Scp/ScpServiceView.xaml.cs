using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels.Scp;
using DesktopApplicationTemplate.UI.Services;

using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.Views.Scp
{
    public partial class ScpServiceView : Page
    {
        private readonly ScpServiceViewModel _viewModel;
        private readonly ILoggingService _logger;
        public ScpServiceView(ScpServiceViewModel vm, ILoggingService logger)
        {
            InitializeComponent();
            _viewModel = vm;
            DataContext = vm;
            _logger = logger;
            _viewModel.Logger = _logger;
        }

    }
}
