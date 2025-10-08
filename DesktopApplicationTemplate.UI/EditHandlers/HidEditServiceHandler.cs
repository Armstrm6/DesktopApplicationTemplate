using System;
using System.Linq;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Hid.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Hid.Advanced;
using DesktopApplicationTemplate.UI.Views.Hid.Edit;
using DesktopApplicationTemplate.UI.Views.Hid.Advanced;
using DesktopApplicationTemplate.Core.Services.Protocols.Hid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class HidEditServiceHandler : IEditServiceHandler
{
    private readonly Func<MainView> _getMainView;
    private readonly Func<MainViewModel> _getMainViewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<HidEditServiceHandler>? _logger;

    public HidEditServiceHandler(Func<MainView> getMainView, Func<MainViewModel> getMainViewModel, IServiceProvider services, ILogger<HidEditServiceHandler>? logger = null)
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
        var hidPage = mainView.GetOrCreateServicePage(service);
        var options = service.HidOptions ?? new HidServiceOptions();
        var vm = ActivatorUtilities.CreateInstance<HidEditServiceViewModel>(_services, service.DisplayName, options);
        var editView = _services.GetRequiredService<HidEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
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
            service.HidOptions = opts;
            if (hidPage != null)
                mainView.ShowPage(hidPage);
            _ = mainViewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (hidPage != null)
                mainView.ShowPage(hidPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<HidAdvancedConfigViewModel>(_services, opts);
            var advView = _services.GetRequiredService<HidAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => mainView.ShowPage(editView);
            advVm.BackRequested += () => mainView.ShowPage(editView);
            mainView.ShowPage(advView);
        };
        mainView.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

