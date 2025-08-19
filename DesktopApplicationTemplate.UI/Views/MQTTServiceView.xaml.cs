using System;
using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;

using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class MQTTServiceView : Page
    {
        private readonly MqttServiceViewModel _viewModel;
        private readonly ILoggingService _logger;
        private readonly Func<MqttEditConnectionView> _editViewFactory;
        public MQTTServiceView(MqttServiceViewModel vm, ILoggingService logger, Func<MqttEditConnectionView> editViewFactory)
        {
            InitializeComponent();
            _viewModel = vm;
            DataContext = vm;
            _logger = logger;
            _editViewFactory = editViewFactory;
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

        private void EditConnection_Click(object sender, RoutedEventArgs e)
        {
            var window = _editViewFactory();
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }
    }
}
