using System;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Http.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Http.Advanced;
using DesktopApplicationTemplate.UI.Views.Http;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class HttpEditServiceHandler : IEditServiceHandler
{
    private readonly MainView _view;
    private readonly MainViewModel _viewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<HttpEditServiceHandler>? _logger;

    public HttpEditServiceHandler(MainView view, MainViewModel viewModel, IServiceProvider services, ILogger<HttpEditServiceHandler>? logger = null)
    {
        _view = view;
        _viewModel = viewModel;
        _services = services;
        _logger = logger;
    }

    public void Edit(ServiceListModel service)
    {
        var httpPage = _view.GetOrCreateServicePage(service);
        var options = service.HttpOptions ?? new HttpServiceOptions();
        var vm = ActivatorUtilities.CreateInstance<HttpEditServiceViewModel>(_services, service.DisplayName.Split(" - ").Last(), options);
        var editView = _services.GetRequiredService<HttpEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"HTTP - {name}";
            service.HttpOptions = opts;
            if (httpPage != null)
                _view.ShowPage(httpPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (httpPage != null)
                _view.ShowPage(httpPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<HttpAdvancedConfigViewModel>(_services, opts);
            var advView = _services.GetRequiredService<HttpAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => _view.ShowPage(editView);
            advVm.BackRequested += () => _view.ShowPage(editView);
            _view.ShowPage(advView);
        };
        _view.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

