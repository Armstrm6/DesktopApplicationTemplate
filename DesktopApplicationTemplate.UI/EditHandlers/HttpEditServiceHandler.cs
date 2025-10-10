using System;
using System.Linq;
using DesktopApplicationTemplate.Core.Services.Protocols.Http;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Http.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Http.Advanced;
using DesktopApplicationTemplate.UI.Views.Http.Edit;
using DesktopApplicationTemplate.UI.Views.Http.Advanced;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class HttpEditServiceHandler : IEditServiceHandler
{
    private readonly Func<MainView> _getMainView;
    private readonly Func<MainViewModel> _getMainViewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<HttpEditServiceHandler>? _logger;

    public HttpEditServiceHandler(Func<MainView> getMainView, Func<MainViewModel> getMainViewModel, IServiceProvider services, ILogger<HttpEditServiceHandler>? logger = null)
    {
        _getMainView = getMainView;
        _getMainViewModel = getMainViewModel;
        _services = services;
        _logger = logger;
    }

    public void Edit(ServiceListModel service)
    {
        var mainView = _getMainView();
        var mainViewModel = _getMainViewModel();
        var httpPage = mainView.GetOrCreateServicePage(service);
        var options = service.GetOptions<HttpServiceOptions>() ?? new HttpServiceOptions();
        service.SetOptions(options);
        var vm = ActivatorUtilities.CreateInstance<HttpEditServiceViewModel>(_services, service.DisplayName, options);
        var editView = _services.GetRequiredService<HttpEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += async (name, opts) =>
        {
            var trimmed = string.IsNullOrWhiteSpace(name)
                ? mainViewModel.GenerateServiceName(service.Type)
                : name.Trim();
            if (mainViewModel.Services.Any(s => s != service && s.DisplayName.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                trimmed = mainViewModel.GenerateServiceName(service.Type);
            }

            var previousName = service.DisplayName;
            if (!string.Equals(previousName, trimmed, StringComparison.Ordinal))
            {
                mainViewModel.ClearRoutingCache(service.Type, previousName);
                mainViewModel.ClearRoutingCache(service.Type, trimmed);
            }

            service.DisplayName = trimmed;
            service.SetOptions(opts);
            if (httpPage != null)
                mainView.ShowPage(httpPage);
            await mainViewModel.SaveServicesAsync().ConfigureAwait(false);
        };
        vm.EditCancelled += () =>
        {
            if (httpPage != null)
                mainView.ShowPage(httpPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<HttpAdvancedConfigViewModel>(_services, opts);
            var advView = _services.GetRequiredService<HttpAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => mainView.ShowPage(editView);
            advVm.BackRequested += () => mainView.ShowPage(editView);
            mainView.ShowPage(advView);
        };
        mainView.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

