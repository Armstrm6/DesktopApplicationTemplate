using System;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Advanced;
using DesktopApplicationTemplate.UI.Views.Scp;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class ScpEditServiceHandler : IEditServiceHandler
{
    private readonly MainView _view;
    private readonly MainViewModel _viewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<ScpEditServiceHandler>? _logger;

    public ScpEditServiceHandler(MainView view, MainViewModel viewModel, IServiceProvider services, ILogger<ScpEditServiceHandler>? logger = null)
    {
        _view = view;
        _viewModel = viewModel;
        _services = services;
        _logger = logger;
    }

    public void Edit(ServiceListModel service)
    {
        var scpPage = _view.GetOrCreateServicePage(service);
        var options = service.ScpOptions ?? new ScpServiceOptions();
        var vm = _services.GetRequiredService<ScpEditServiceViewModel>();
        vm.Load(service.DisplayName.Split(" - ").Last(), options);
        var editView = _services.GetRequiredService<ScpEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"SCP - {name}";
            service.ScpOptions = opts;
            if (scpPage != null)
                _view.ShowPage(scpPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (scpPage != null)
                _view.ShowPage(scpPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = _services.GetRequiredService<ScpAdvancedConfigViewModel>();
            advVm.Load(opts);
            var advView = _services.GetRequiredService<ScpAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => _view.ShowPage(editView);
            advVm.BackRequested += () => _view.ShowPage(editView);
            _view.ShowPage(advView);
        };
        _view.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

