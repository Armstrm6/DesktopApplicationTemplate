using System;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Hid;
using DesktopApplicationTemplate.UI.Views.Hid;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class HidEditServiceHandler : IEditServiceHandler
{
    private readonly MainView _view;
    private readonly MainViewModel _viewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<HidEditServiceHandler>? _logger;

    public HidEditServiceHandler(MainView view, MainViewModel viewModel, IServiceProvider services, ILogger<HidEditServiceHandler>? logger = null)
    {
        _view = view;
        _viewModel = viewModel;
        _services = services;
        _logger = logger;
    }

    public void Edit(ServiceListModel service)
    {
        var hidPage = _view.GetOrCreateServicePage(service);
        var options = service.HidOptions ?? new HidServiceOptions();
        var vm = ActivatorUtilities.CreateInstance<HidEditServiceViewModel>(_services, service.DisplayName.Split(" - ").Last(), options);
        var editView = _services.GetRequiredService<HidEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"HID - {name}";
            service.HidOptions = opts;
            if (hidPage != null)
                _view.ShowPage(hidPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (hidPage != null)
                _view.ShowPage(hidPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<HidAdvancedConfigViewModel>(_services, opts);
            var advView = _services.GetRequiredService<HidAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => _view.ShowPage(editView);
            advVm.BackRequested += () => _view.ShowPage(editView);
            _view.ShowPage(advView);
        };
        _view.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

