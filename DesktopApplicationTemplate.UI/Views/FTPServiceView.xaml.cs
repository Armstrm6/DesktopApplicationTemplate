using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    /// <summary>
    /// View displaying FTP server status and transfers.
    /// </summary>
    public partial class FTPServiceView : Page
    {
        public FTPServiceView(FtpServiceViewModel viewModel, ILoggingService logger)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.Logger = logger;
        }
    }
}
