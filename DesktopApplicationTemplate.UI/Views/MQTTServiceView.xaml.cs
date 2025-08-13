using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class MQTTServiceView : Page
    {
        private readonly MqttServiceViewModel _viewModel;
        private readonly ILoggingService _logger;
        public MQTTServiceView(MqttServiceViewModel vm, ILoggingService logger)
        {
            InitializeComponent();
            _viewModel = vm;
            DataContext = vm;
            _logger = logger;
            (logger as LoggingService)?.Initialize(LogBox, Dispatcher);
            _viewModel.Logger = _logger;
        }

        private void Help_Click(object sender, System.Windows.RoutedEventArgs e)
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
