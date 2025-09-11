using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Views
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
            Enum.TryParse<ServiceType>(service.ServiceType, true, out var type);
            LogView.DataContext = new ServiceLogViewModel(type, service.Logs);
        }
    }
}
