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
            };
            _page.Cancelled += () => DialogResult = false;
            _page.MqttSelected += NavigateToMqtt;
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
            };
            vm.Cancelled += () => ContentFrame.Content = _page;
            var view = ActivatorUtilities.CreateInstance<MqttCreateServiceView>(_services, vm);
            ContentFrame.Content = view;
        }
    }
}
