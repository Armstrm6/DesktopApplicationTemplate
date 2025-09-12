using System;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Tcp;
using DesktopApplicationTemplate.UI.Views.Tcp;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class TcpEditServiceHandler : IEditServiceHandler
{
    private readonly MainView _view;
    private readonly MainViewModel _viewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<TcpEditServiceHandler>? _logger;

    public TcpEditServiceHandler(MainView view, MainViewModel viewModel, IServiceProvider services, ILogger<TcpEditServiceHandler>? logger = null)
    {
        _view = view;
        _viewModel = viewModel;
        _services = services;
        _logger = logger;
    }

    public void Edit(ServiceListModel service)
    {
        var tcpPage = _view.GetOrCreateServicePage(service);
        var options = service.TcpOptions ?? new TcpServiceOptions();
        var vm = _services.GetRequiredService<TcpEditServiceViewModel>();
        vm.ServiceType = service.Type;
        vm.Load(service.DisplayName.Split(" - ").Last(), options);
        var editView = _services.GetRequiredService<TcpEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"{vm.ServiceType.ToLegacyString()} - {name}";
            service.Type = vm.ServiceType;
            service.TcpOptions = opts;
            if (tcpPage != null)
                _view.ShowPage(tcpPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (tcpPage != null)
                _view.ShowPage(tcpPage);
        };
        _view.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

