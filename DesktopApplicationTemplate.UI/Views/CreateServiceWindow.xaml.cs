using System;
using System.Windows;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class CreateServiceWindow : Window
    {
        public string CreatedServiceName { get; private set; } = string.Empty;
        public string CreatedServiceType { get; private set; } = string.Empty;
        public MqttServiceOptions? MqttOptions { get; private set; }
        public TcpServiceOptions? TcpOptions { get; private set; }
        public FtpServerOptions? FtpServerOptions { get; private set; }

        private readonly IServiceProvider _services;
        private readonly CreateServicePage _page;

        public CreateServiceWindow(CreateServiceViewModel viewModel, IServiceProvider services)
        {
            InitializeComponent();
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _page = new CreateServicePage(viewModel);
            _page.ServiceCreated += (name, type) =>
            {
                CreatedServiceName = name;
                CreatedServiceType = type;
                DialogResult = true;
                Close();
            };
            _page.Cancelled += () =>
            {
                DialogResult = false;
                Close();
            };
            _page.MqttSelected += NavigateToMqtt;
            _page.TcpSelected += NavigateToTcp;
            _page.FtpServerSelected += NavigateToFtpServer;
            ContentFrame.Content = _page;
        }

        private void NavigateToMqtt(string defaultName)
        {
            var vm = _services.GetRequiredService<MqttCreateServiceViewModel>();
            vm.ServiceName = defaultName;
            vm.ServiceCreated += (name, options) =>
            {
                CreatedServiceName = name;
                CreatedServiceType = "MQTT";
                MqttOptions = options;
                DialogResult = true;
                Close();
            };
            vm.Cancelled += () => ContentFrame.Content = _page;
            var view = ActivatorUtilities.CreateInstance<MqttCreateServiceView>(_services, vm);
            ContentFrame.Content = view;
        }

        private void NavigateToTcp(string defaultName)
        {
            var vm = _services.GetRequiredService<TcpCreateServiceViewModel>();
            vm.ServiceName = defaultName;
            vm.ServiceCreated += (name, options) =>
            {
                CreatedServiceName = name;
                CreatedServiceType = "TCP";
                TcpOptions = options;
                DialogResult = true;
                Close();
            };
            vm.Cancelled += () => ContentFrame.Content = _page;
            var view = _services.GetRequiredService<TcpCreateServiceView>();
            view.DataContext = vm;
            ContentFrame.Content = view;
        }

        private void NavigateToFtpServer(string defaultName)
        {
            var vm = _services.GetRequiredService<FtpServerCreateViewModel>();
            vm.ServiceName = defaultName;
            var view = _services.GetRequiredService<FtpServerCreateView>();
            view.DataContext = vm;
            vm.ServerCreated += (name, options) =>
            {
                CreatedServiceName = name;
                CreatedServiceType = "FTP Server";
                FtpServerOptions = options;
                DialogResult = true;
                Close();
            };
            vm.AdvancedConfigRequested += opts =>
            {
                var advVm = ActivatorUtilities.CreateInstance<FtpServerAdvancedConfigViewModel>(_services, opts);
                var advView = _services.GetRequiredService<FtpServerAdvancedConfigView>();
                advView.DataContext = advVm;
                advVm.Saved += _ => ContentFrame.Content = view;
                advVm.Cancelled += () => ContentFrame.Content = view;
                ContentFrame.Content = advView;
            };
            ContentFrame.Content = view;
        }
    }
}
