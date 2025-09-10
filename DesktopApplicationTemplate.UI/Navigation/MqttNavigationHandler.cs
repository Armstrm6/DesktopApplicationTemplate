using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models = DesktopApplicationTemplate.Models;
using Microsoft.Extensions.Options;

namespace DesktopApplicationTemplate.UI.Navigation
{
    public class MqttNavigationHandler : INavigationHandler
    {
        private readonly IServiceProvider _services;
        private readonly MainViewModel _mainViewModel;
        private readonly MainView _mainView;
        private readonly ILogger<MainView>? _logger;

        public string ServiceType => "MQTT";

        public MqttNavigationHandler(IServiceProvider services,
            MainViewModel mainViewModel,
            MainView mainView,
            ILogger<MainView>? logger = null)
        {
            _services = services;
            _mainViewModel = mainViewModel;
            _mainView = mainView;
            _logger = logger;
        }

        public Page CreateView(string defaultName)
        {
            var vm = _services.GetRequiredService<MqttCreateServiceViewModel>();
            vm.ServiceName = defaultName;
            vm.ServiceSaved += async (name, options) => await AddServiceAsync(name, options);
            vm.EditCancelled += _mainView.ShowCreateServiceSelectionPage;
            var view = ActivatorUtilities.CreateInstance<MqttCreateServiceView>(_services, vm);
            vm.AdvancedConfigRequested += opts =>
            {
                var advVm = ActivatorUtilities.CreateInstance<MqttAdvancedConfigViewModel>(_services, opts);
                var advView = _services.GetRequiredService<MqttAdvancedConfigView>();
                advView.Initialize(advVm);
                advVm.Saved += _ => _mainView.ShowPage(view);
                advVm.BackRequested += () => _mainView.ShowPage(view);
                _mainView.ShowPage(advView);
            };
            return view;
        }

        public async Task AddServiceAsync(string name, object optionsObj)
        {
            var options = (MqttServiceOptions)optionsObj;
            var newService = new Models.ServiceListModel
            {
                DisplayName = $"MQTT - {name}",
                ServiceType = "MQTT",
                IsActive = false
            };
            newService.SetColorsByType();
            newService.LogAdded += _mainViewModel.OnServiceLogAdded;
            newService.ActiveChanged += _mainViewModel.OnServiceActiveChanged;
            _mainView.GetOrCreateServicePage(newService);
            var opt = _services.GetRequiredService<IOptions<MqttServiceOptions>>().Value;
            opt.Host = options.Host;
            opt.Port = options.Port;
            opt.ClientId = options.ClientId;
            opt.Username = options.Username;
            opt.Password = options.Password;
            opt.ConnectionType = options.ConnectionType;
            opt.WillTopic = options.WillTopic;
            opt.WillPayload = options.WillPayload;
            opt.WillQualityOfService = options.WillQualityOfService;
            opt.WillRetain = options.WillRetain;
            opt.KeepAliveSeconds = options.KeepAliveSeconds;
            opt.CleanSession = options.CleanSession;
            opt.ReconnectDelay = options.ReconnectDelay;
            _mainViewModel.Services.Add(newService);
            _logger?.LogInformation("Service {Name} added", newService.DisplayName);
            _mainViewModel.SelectedService = newService;
            _mainView.ServiceList.ScrollIntoView(newService);
            if (newService.ServicePage is MqttTagSubscriptionsView mqttView)
            {
                var mqttVm = (MqttTagSubscriptionsViewModel)mqttView.DataContext!;
                newService.ActiveChanged += active =>
                {
                    if (active)
                    {
                        _ = mqttVm.ConnectAsync();
                    }
                };
                mqttVm.EditConnectionRequested += (_, _) =>
                {
                    var editView = _services.GetRequiredService<MqttEditConnectionView>();
                    if (editView.DataContext is MqttEditConnectionViewModel vm)
                    {
                        var opt = _services.GetRequiredService<IOptions<MqttServiceOptions>>().Value;
                        vm.Load(opt);
                        vm.HighlightMissingFields();
                        vm.RequestClose += (_, _) =>
                        {
                            if (newService.ServicePage != null)
                                _mainView.ShowPage(newService.ServicePage);
                            _ = _mainViewModel.SaveServicesAsync();
                        };
                    }
                    _mainView.ShowPage(editView);
                };
            }
            if (newService.ServicePage != null)
            {
                _mainView.ShowPage(newService.ServicePage);
            }
            await _mainViewModel.SaveServicesAsync();
            _logger?.LogDebug("AddService workflow completed");
        }
    }
}
