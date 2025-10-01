using System;
using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Http.UI.EditHandlers;
using DesktopApplicationTemplate.Services.Http.UI.Factories;
using DesktopApplicationTemplate.Services.Http.UI.Navigation;
using DesktopApplicationTemplate.Services.Http.UI.Services;
using DesktopApplicationTemplate.Services.Http.UI.ViewModels.Http;
using DesktopApplicationTemplate.Services.Http.UI.ViewModels.Http.Advanced;
using DesktopApplicationTemplate.Services.Http.UI.ViewModels.Http.Create;
using DesktopApplicationTemplate.Services.Http.UI.ViewModels.Http.Edit;
using DesktopApplicationTemplate.Services.Http.UI.Views.Http;
using DesktopApplicationTemplate.Services.Http.UI.Views.Http.Advanced;
using DesktopApplicationTemplate.Services.Http.UI.Views.Http.Create;
using DesktopApplicationTemplate.Services.Http.UI.Views.Http.Edit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Services.Http.UI.Modules;

public sealed class HttpUiModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services
            .AddOptions<HttpServiceOptions>()
            .BindConfiguration("HttpService");

        services.AddTransient<HttpServiceFactory>();
        services.AddTransient<HttpNavigationHandler>();
        services.AddTransient<HttpEditServiceHandler>();

        services.AddTransient<HttpServiceViewModel>();
        services.AddTransient<HttpCreateServiceViewModel>();
        services.AddTransient<HttpEditServiceViewModel>();
        services.AddTransient<HttpAdvancedConfigViewModel>();

        services.AddTransient<HttpCreateServiceView>();
        services.AddTransient<HttpEditServiceView>();
        services.AddTransient<HttpAdvancedConfigView>();
        services.AddTransient<HttpServiceView>();
    }

    public IEnumerable<IServiceDescriptor> DescribeServices() => Array.Empty<IServiceDescriptor>();
}
