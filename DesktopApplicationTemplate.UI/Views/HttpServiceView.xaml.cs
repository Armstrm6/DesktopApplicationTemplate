using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.Views
{
    /// <summary>
    /// Interaction logic for HttpServiceView.xaml
    /// </summary>
    public partial class HttpServiceView : Page
    {
        private readonly ViewModels.HttpServiceViewModel _viewModel;
        private readonly ILoggingService _logger;

        public HttpServiceView(ViewModels.HttpServiceViewModel viewModel, ILoggingService logger)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            _logger = logger;
            (logger as LoggingService)?.Initialize(LogBox, Dispatcher);
            _viewModel.Logger = _logger;
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var help = new AsciiHelpWindow();
            help.ShowDialog();
        }

        private void LogLevelBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
    }
}
