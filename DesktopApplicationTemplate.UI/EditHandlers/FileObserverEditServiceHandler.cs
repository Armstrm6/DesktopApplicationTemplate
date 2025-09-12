using System;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.FileObserver.Edit;
using DesktopApplicationTemplate.UI.ViewModels.FileObserver.Advanced;
using DesktopApplicationTemplate.UI.Views.FileObserver.Edit;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class FileObserverEditServiceHandler : IEditServiceHandler
{
    private readonly MainView _view;
    private readonly MainViewModel _viewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<FileObserverEditServiceHandler>? _logger;

    public FileObserverEditServiceHandler(MainView view, MainViewModel viewModel, IServiceProvider services, ILogger<FileObserverEditServiceHandler>? logger = null)
    {
        _view = view;
        _viewModel = viewModel;
        _services = services;
        _logger = logger;
    }

    public void Edit(ServiceListModel service)
    {
        var foPage = _view.GetOrCreateServicePage(service);
        var options = service.FileObserverOptions ?? new FileObserverServiceOptions();
        var vm = ActivatorUtilities.CreateInstance<FileObserverEditServiceViewModel>(_services, service.DisplayName.Split(" - ").Last(), options);
        var editView = _services.GetRequiredService<FileObserverEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"File Observer - {name}";
            service.FileObserverOptions = opts;
            if (foPage != null)
                _view.ShowPage(foPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (foPage != null)
                _view.ShowPage(foPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<FileObserverAdvancedConfigViewModel>(_services, opts);
            var advView = _services.GetRequiredService<FileObserverAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => _view.ShowPage(editView);
            advVm.BackRequested += () => _view.ShowPage(editView);
            _view.ShowPage(advView);
        };
        _view.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

