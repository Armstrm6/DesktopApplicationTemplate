using System;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Ftp.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Ftp.Advanced;
using DesktopApplicationTemplate.UI.Views.Ftp;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class FtpEditServiceHandler : IEditServiceHandler
{
    private readonly MainView _view;
    private readonly MainViewModel _viewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<FtpEditServiceHandler>? _logger;

    public FtpEditServiceHandler(MainView view, MainViewModel viewModel, IServiceProvider services, ILogger<FtpEditServiceHandler>? logger = null)
    {
        _view = view;
        _viewModel = viewModel;
        _services = services;
        _logger = logger;
    }

    public void Edit(ServiceListModel service)
    {
        var ftpPage = _view.GetOrCreateServicePage(service);
        var options = service.FtpOptions ?? new FtpServerOptions();
        var vm = ActivatorUtilities.CreateInstance<FtpServerEditViewModel>(_services, service.DisplayName.Split(" - ").Last(), options);
        var editView = ActivatorUtilities.CreateInstance<FtpServerEditView>(_services, vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"FTP Server - {name}";
            service.FtpOptions = opts;
            var opt = _services.GetRequiredService<IOptions<FtpServerOptions>>().Value;
            opt.Port = opts.Port;
            opt.RootPath = opts.RootPath;
            opt.AllowAnonymous = opts.AllowAnonymous;
            opt.Username = opts.Username;
            opt.Password = opts.Password;
            if (ftpPage != null)
                _view.ShowPage(ftpPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (ftpPage != null)
                _view.ShowPage(ftpPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<FtpServerAdvancedConfigViewModel>(_services, opts);
            var advView = _services.GetRequiredService<FtpServerAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => _view.ShowPage(editView);
            advVm.BackRequested += () => _view.ShowPage(editView);
            _view.ShowPage(advView);
        };
        _view.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

