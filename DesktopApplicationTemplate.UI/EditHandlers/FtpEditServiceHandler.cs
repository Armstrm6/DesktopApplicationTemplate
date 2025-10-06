using System;
using DesktopApplicationTemplate.Core.Services.Protocols.Ftp;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Ftp.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Ftp.Advanced;
using DesktopApplicationTemplate.UI.Views.Ftp.Edit;
using DesktopApplicationTemplate.UI.Views.Ftp.Advanced;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class FtpEditServiceHandler : IEditServiceHandler
{
    private readonly Func<MainView> _getMainView;
    private readonly Func<MainViewModel> _getMainViewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<FtpEditServiceHandler>? _logger;

    public FtpEditServiceHandler(Func<MainView> getMainView, Func<MainViewModel> getMainViewModel, IServiceProvider services, ILogger<FtpEditServiceHandler>? logger = null)
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
        var ftpPage = mainView.GetOrCreateServicePage(service);
        var options = service.FtpOptions ?? new FtpServerOptions();
        var vm = ActivatorUtilities.CreateInstance<FtpServerEditViewModel>(_services, service.DisplayName.Split(" - ").Last(), options);
        var editView = ActivatorUtilities.CreateInstance<FtpServerEditView>(_services, vm);
        vm.ServiceSaved += (name, opts) =>
        {
            var finalName = mainViewModel.EnsureUniqueServiceName(service, name);
            service.DisplayName = $"FTP Server - {finalName}";
            service.FtpOptions = opts;
            var opt = _services.GetRequiredService<IOptions<FtpServerOptions>>().Value;
            opt.Port = opts.Port;
            opt.RootPath = opts.RootPath;
            opt.AllowAnonymous = opts.AllowAnonymous;
            opt.Username = opts.Username;
            opt.Password = opts.Password;
            if (ftpPage != null)
                mainView.ShowPage(ftpPage);
            _ = mainViewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (ftpPage != null)
                mainView.ShowPage(ftpPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<FtpServerAdvancedConfigViewModel>(_services, opts);
            var advView = _services.GetRequiredService<FtpServerAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => mainView.ShowPage(editView);
            advVm.BackRequested += () => mainView.ShowPage(editView);
            mainView.ShowPage(advView);
        };
        mainView.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

