using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class FTPServiceView : Page
    {
        private readonly FtpServiceViewModel _viewModel;
        public FTPServiceView(FtpServiceViewModel vm)
        {
            InitializeComponent();
            _viewModel = vm;
            DataContext = vm;
            _viewModel.Logger = new LoggingService(LogBox, Dispatcher);
        }
    }
}
