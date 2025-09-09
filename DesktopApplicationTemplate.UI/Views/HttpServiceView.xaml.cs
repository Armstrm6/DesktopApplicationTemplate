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
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.Views
{
    /// <summary>
    /// Interaction logic for HttpServiceView.xaml
    /// </summary>
    public partial class HttpServiceView : Page, IServiceLogHost
    {
        private readonly ViewModels.HttpServiceViewModel _viewModel;
        private readonly ILoggingService _logger;

        public HttpServiceView()
            : this(App.AppHost.Services.GetRequiredService<ViewModels.HttpServiceViewModel>(),
                   App.AppHost.Services.GetRequiredService<ILoggingService>())
        {
        }

        public HttpServiceView(ViewModels.HttpServiceViewModel viewModel, ILoggingService logger)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            _logger = logger;
            _viewModel.Logger = _logger;
        }

        public void SetServiceContext(ServiceViewModel service)
        {
            LogView.DataContext = new ServiceLogViewModel(service.DisplayName, service.ServiceType, service.Logs);
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var help = new AsciiHelpWindow();
            help.ShowDialog();
        }

    }
}
