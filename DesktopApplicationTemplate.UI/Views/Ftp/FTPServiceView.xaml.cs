using DesktopApplicationTemplate.UI.ViewModels;
using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Ftp;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Views.Ftp
{
    /// <summary>
    /// View displaying FTP server status and transfers.
    /// </summary>
    public partial class FTPServiceView : Page, IServiceLogHost
    {
        public FTPServiceView(FtpServiceViewModel viewModel, ILoggingService logger)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.Logger = logger;
        }

        public void SetServiceContext(ServiceListModel service)
        {
            LogView.DataContext = new ServiceLogViewModel(service.Type, service.LogState.Logs);
        }
    }
}
