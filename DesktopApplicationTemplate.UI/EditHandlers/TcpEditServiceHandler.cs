using System;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Tcp.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Tcp.Advanced;
using DesktopApplicationTemplate.UI.Views.Tcp.Edit;
using DesktopApplicationTemplate.UI.Views.Tcp.Advanced;
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
        var tcpPage = mainView.GetOrCreateServicePage(service);
        var options = service.TcpOptions ?? new TcpServiceOptions();
        var vm = _services.GetRequiredService<TcpEditServiceViewModel>();
        vm.ServiceType = service.Type;
        vm.Load(service.DisplayName.Split(" - ").Last(), options);
        var editView = _services.GetRequiredService<TcpEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            var finalName = mainViewModel.EnsureUniqueServiceName(service, name);
            service.DisplayName = $"{vm.ServiceType.ToLegacyString()} - {finalName}";
            service.Type = vm.ServiceType;
            service.TcpOptions = opts;
            if (tcpPage != null)
                mainView.ShowPage(tcpPage);
            _ = mainViewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (tcpPage != null)
                mainView.ShowPage(tcpPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<TcpAdvancedConfigViewModel>(_services, opts);
            var advView = _services.GetRequiredService<TcpAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => mainView.ShowPage(editView);
            advVm.BackRequested += () => mainView.ShowPage(editView);
            mainView.ShowPage(advView);
        };
        mainView.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

