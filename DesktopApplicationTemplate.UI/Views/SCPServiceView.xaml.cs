using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class SCPServiceView : Page
    {
        private readonly ScpServiceViewModel _viewModel;
        public SCPServiceView(ScpServiceViewModel vm)
        {
            InitializeComponent();
            _viewModel = vm;
            DataContext = vm;
            _viewModel.Logger = new LoggingService(LogBox, Dispatcher);
        }
    }
}
