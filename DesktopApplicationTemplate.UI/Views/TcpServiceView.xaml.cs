using System.Windows;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.Core.Services;
using System.Windows.Controls;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class TcpServiceView : Page
    {
        private readonly TcpServiceViewModel _viewModel;
        private readonly IStartupService _startupService;
        private readonly ILoggingService _logger;

        public TcpServiceView(TcpServiceViewModel viewModel, IStartupService startupService, ILoggingService logger)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _startupService = startupService;
            _logger = logger;

            DataContext = _viewModel;
            _viewModel.Logger = _logger;

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
            if (LogLevelBox.SelectedItem is ComboBoxItem item)
            {
                if (_logger is LoggingService concrete)
                {
                    switch (item.Content?.ToString())
                    {
                        case "Warning":
                            concrete.MinimumLevel = LogLevel.Warning;
                            break;
                        case "Error":
                            concrete.MinimumLevel = LogLevel.Error;
                            break;
                        case "Debug":
                            concrete.MinimumLevel = LogLevel.Debug;
                            break;
                        default:
                            concrete.MinimumLevel = LogLevel.Debug;
                            break;
                    }
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
