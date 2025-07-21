using System.Windows;
using DesktopApplicationTemplate.ViewModels;
using DesktopApplicationTemplate.Services;
using System.Windows.Controls;

namespace DesktopApplicationTemplate.Views
{
    public partial class TcpServiceView : Page
    {
        private readonly TcpServiceViewModel _viewModel;
        private readonly IStartupService _startupService;

        public TcpServiceView(TcpServiceViewModel viewModel, IStartupService startupService)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _startupService = startupService;

            DataContext = _viewModel;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _startupService.RunStartupChecksAsync();
        }
    }
}
