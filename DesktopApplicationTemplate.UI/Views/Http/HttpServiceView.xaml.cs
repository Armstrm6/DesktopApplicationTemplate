using DesktopApplicationTemplate.UI.ViewModels;
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
using DesktopApplicationTemplate.UI.ViewModels.Http;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.Views.Http
{
    /// <summary>
    /// Interaction logic for HttpServiceView.xaml
    /// </summary>
    public partial class HttpServiceView : Page, IServiceLogHost
    {
        private readonly HttpServiceViewModel _viewModel;
        private readonly ILoggingService _logger;

        public HttpServiceView()
            : this(App.AppHost.Services.GetRequiredService<HttpServiceViewModel>(),
                   App.AppHost.Services.GetRequiredService<ILoggingService>())
        {
        }

        public HttpServiceView(HttpServiceViewModel viewModel, ILoggingService logger)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            _logger = logger;
            _viewModel.Logger = _logger;
        }

        public void SetServiceContext(ServiceListModel service)
        {
            LogView.DataContext = new ServiceLogViewModel(service.Type, service.Logs);
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var help = new AsciiHelpWindow();
            help.ShowDialog();
        }

    }
}
