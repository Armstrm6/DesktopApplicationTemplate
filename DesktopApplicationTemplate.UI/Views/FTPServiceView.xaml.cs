using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;

using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class FTPServiceView : Page
    {
        private readonly FtpServiceViewModel _viewModel;
        private readonly ILoggingService _logger;
        public FTPServiceView(FtpServiceViewModel vm, ILoggingService logger)
        {
            InitializeComponent();
            _viewModel = vm;
            DataContext = vm;
            _logger = logger;
            _viewModel.Logger = _logger;
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
