using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DesktopApplicationTemplate.UI.Services;

using DesktopApplicationTemplate.Core.Services;

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
    }
}
