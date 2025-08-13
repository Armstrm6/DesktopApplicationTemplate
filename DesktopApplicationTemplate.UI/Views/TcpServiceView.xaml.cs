using System.Windows;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Helpers;
using System.Windows.Controls;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class TcpServiceView : Page
    {
        private readonly TcpServiceViewModel _viewModel;
        private readonly IStartupService _startupService;
        private readonly LoggingService _logger;

        public TcpServiceView(TcpServiceViewModel viewModel, IStartupService startupService)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _startupService = startupService;

            DataContext = _viewModel;
            _logger = new LoggingService(LogBox, Dispatcher);
            _viewModel.Logger = _logger;
            SaveConfirmationHelper.Logger = _logger;
            CloseConfirmationHelper.Logger = _logger;

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

        private void LogLevelBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_logger == null)
                return;

            if (LogLevelBox.SelectedItem is ComboBoxItem item)
            {
                switch (item.Content?.ToString())
                {
                    case "Warning":
                        _logger.MinimumLevel = LogLevel.Warning;
                        break;
                    case "Error":
                        _logger.MinimumLevel = LogLevel.Error;
                        break;
                    case "Debug":
                        _logger.MinimumLevel = LogLevel.Debug;
                        break;
                    default:
                        _logger.MinimumLevel = LogLevel.Debug;
                        break;
                }
            }
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var help = new AsciiHelpWindow();
            help.ShowDialog();
        }
    }
}
