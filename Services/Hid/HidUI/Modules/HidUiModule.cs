using System;
using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Hid.UI.EditHandlers;
using DesktopApplicationTemplate.Services.Hid.UI.Factories;
using DesktopApplicationTemplate.Services.Hid.UI.Navigation;
using DesktopApplicationTemplate.Services.Hid.UI.Services;
using DesktopApplicationTemplate.Services.Hid.UI.ViewModels.Hid;
using DesktopApplicationTemplate.Services.Hid.UI.ViewModels.Hid.Advanced;
using DesktopApplicationTemplate.Services.Hid.UI.ViewModels.Hid.Create;
using DesktopApplicationTemplate.Services.Hid.UI.ViewModels.Hid.Edit;
using DesktopApplicationTemplate.Services.Hid.UI.Views.Hid;
using DesktopApplicationTemplate.Services.Hid.UI.Views.Hid.Advanced;
using DesktopApplicationTemplate.Services.Hid.UI.Views.Hid.Create;
using DesktopApplicationTemplate.Services.Hid.UI.Views.Hid.Edit;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Services.Hid.UI.Modules;

/// <summary>
/// Registers HID UI services, view models, and views.
/// </summary>
public sealed class HidUiModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<HidServiceFactory>();
        services.AddTransient<HidNavigationHandler>();
        services.AddTransient<HidEditServiceHandler>();

        services.AddTransient<HidViewModel>();
        services.AddTransient<HidCreateServiceViewModel>();
        services.AddTransient<HidEditServiceViewModel>();
        services.AddTransient<HidAdvancedConfigViewModel>();

        services.AddTransient<HidViews>();
        services.AddTransient<HidCreateServiceView>();
        services.AddTransient<HidEditServiceView>();
        services.AddTransient<HidAdvancedConfigView>();

        services.AddSingleton<IHookReleaseService, KeyboardSimulatorHookReleaseService>();
    }

    public IEnumerable<IServiceDescriptor> DescribeServices() => Array.Empty<IServiceDescriptor>();
}
