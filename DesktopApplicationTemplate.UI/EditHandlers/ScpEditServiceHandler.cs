using System;
using System.Linq;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Advanced;
using DesktopApplicationTemplate.UI.Views.Scp.Edit;
using DesktopApplicationTemplate.UI.Views.Scp.Advanced;
using DesktopApplicationTemplate.Core.Services.Protocols.Scp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class ScpEditServiceHandler : IEditServiceHandler
{
    private readonly Func<MainView> _getMainView;
    private readonly Func<MainViewModel> _getMainViewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<ScpEditServiceHandler>? _logger;

    public ScpEditServiceHandler(Func<MainView> getMainView, Func<MainViewModel> getMainViewModel, IServiceProvider services, ILogger<ScpEditServiceHandler>? logger = null)
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
        var scpPage = mainView.GetOrCreateServicePage(service);
        var options = service.GetOrCreateOptions(() => new ScpServiceOptions());
        var vm = _services.GetRequiredService<ScpEditServiceViewModel>();
        vm.Load(service.DisplayName, options);
        var editView = _services.GetRequiredService<ScpEditServiceView>();
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
            service.SetOptions((ScpServiceOptions)opts);
            if (scpPage != null)
                mainView.ShowPage(scpPage);
            _ = mainViewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (scpPage != null)
                mainView.ShowPage(scpPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = _services.GetRequiredService<ScpAdvancedConfigViewModel>();
            advVm.Load(opts);
            var advView = _services.GetRequiredService<ScpAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => mainView.ShowPage(editView);
            advVm.BackRequested += () => mainView.ShowPage(editView);
            mainView.ShowPage(advView);
        };
        mainView.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

