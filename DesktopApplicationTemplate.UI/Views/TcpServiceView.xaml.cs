using System.Windows;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Views;
using System.Windows.Controls;

namespace DesktopApplicationTemplate.UI.Views
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
            _viewModel.Logger = new LoggingService(LogBox, Dispatcher);

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _startupService.RunStartupChecksAsync();
        }

        private void OpenEditor_Click(object sender, RoutedEventArgs e)
        {
            var editor = new ScriptEditorWindow(_viewModel.ScriptContent);
            if (editor.ShowDialog() == true)
            {
                _viewModel.ScriptContent = editor.ScriptText;
            }
        }
    }
}
