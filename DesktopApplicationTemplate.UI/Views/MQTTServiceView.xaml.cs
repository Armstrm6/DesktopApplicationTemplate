using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Helpers;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class MQTTServiceView : Page
    {
        private readonly MqttServiceViewModel _viewModel;
        private readonly LoggingService _logger;
        public MQTTServiceView(MqttServiceViewModel vm)
        {
            InitializeComponent();
            _viewModel = vm;
            DataContext = vm;
            _logger = new LoggingService(LogBox, Dispatcher);
            _viewModel.Logger = _logger;
            SaveConfirmationHelper.Logger = _logger;
            CloseConfirmationHelper.Logger = _logger;
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
