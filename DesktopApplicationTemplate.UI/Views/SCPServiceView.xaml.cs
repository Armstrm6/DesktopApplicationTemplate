using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;

using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class SCPServiceView : Page
    {
        private readonly ScpServiceViewModel _viewModel;
        private readonly ILoggingService _logger;
        public SCPServiceView(ScpServiceViewModel vm, ILoggingService logger)
        {
            InitializeComponent();
            _viewModel = vm;
            DataContext = vm;
            _logger = logger;
            _viewModel.Logger = _logger;
        }

    }
}
