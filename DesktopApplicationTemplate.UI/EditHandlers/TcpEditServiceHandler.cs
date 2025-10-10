using System;
using System.Linq;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Tcp.Edit;
using DesktopApplicationTemplate.UI.Views.Tcp.Edit;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class TcpEditServiceHandler : IEditServiceHandler
{
    private readonly Func<MainView> _getMainView;
    private readonly Func<MainViewModel> _getMainViewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<TcpEditServiceHandler>? _logger;

    public TcpEditServiceHandler(Func<MainView> getMainView, Func<MainViewModel> getMainViewModel, IServiceProvider services, ILogger<TcpEditServiceHandler>? logger = null)
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
        var options = service.GetOptions<TcpServiceOptions>() ?? new TcpServiceOptions();
        service.SetOptions(options);
        var vm = _services.GetRequiredService<TcpEditServiceViewModel>();
        vm.ServiceType = service.Type;
        vm.Load(service.DisplayName, options);
        var editView = _services.GetRequiredService<TcpEditServiceView>();
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
            var previousType = service.Type;
            var newType = vm.ServiceType;
            var nameChanged = !string.Equals(previousName, trimmed, StringComparison.Ordinal);
            var typeChanged = previousType != newType;
            if (nameChanged || typeChanged)
            {
                mainViewModel.ClearRoutingCache(previousType, previousName);
                mainViewModel.ClearRoutingCache(newType, trimmed);
            }

            service.DisplayName = trimmed;
            service.Type = newType;
            service.SetOptions(opts);
            var page = mainView.GetOrCreateServicePage(service);
            if (page != null)
            {
                mainView.ShowPage(page);
            }
            mainViewModel.SelectedService = service;
            await mainViewModel.SaveServicesAsync().ConfigureAwait(false);
        };
        vm.EditCancelled += () =>
        {
            var page = mainView.GetOrCreateServicePage(service);
            if (page != null)
            {
                mainView.ShowPage(page);
            }
            mainViewModel.SelectedService = service;
        };
        mainView.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

