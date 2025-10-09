using System;
using System.Linq;
using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Csv.Edit;
using DesktopApplicationTemplate.UI.Views.Csv.Edit;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class CsvEditServiceHandler : IEditServiceHandler
{
    private readonly Func<MainView> _getMainView;
    private readonly Func<MainViewModel> _getMainViewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<CsvEditServiceHandler>? _logger;

    public CsvEditServiceHandler(Func<MainView> getMainView, Func<MainViewModel> getMainViewModel, IServiceProvider services, ILogger<CsvEditServiceHandler>? logger = null)
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
        var csvPage = mainView.GetOrCreateServicePage(service);
        var options = service.GetOrCreateOptions(() => new CsvServiceOptions());
        var vm = _services.GetRequiredService<CsvServiceEditorViewModel>();
        vm.Load(service.DisplayName, options);
        var editView = _services.GetRequiredService<CsvServiceEditorView>();
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
            service.SetOptions((CsvServiceOptions)opts);
            if (csvPage != null)
                mainView.ShowPage(csvPage);
            _ = mainViewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (csvPage != null)
                mainView.ShowPage(csvPage);
        };
        mainView.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

