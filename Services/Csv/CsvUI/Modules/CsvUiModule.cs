using System;
using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Csv.UI.EditHandlers;
using DesktopApplicationTemplate.Services.Csv.UI.Factories;
using DesktopApplicationTemplate.Services.Csv.UI.Navigation;
using DesktopApplicationTemplate.Services.Csv.UI.Services;
using DesktopApplicationTemplate.Services.Csv.UI.ViewModels.Csv;
using DesktopApplicationTemplate.Services.Csv.UI.ViewModels.Csv.Advanced;
using DesktopApplicationTemplate.Services.Csv.UI.ViewModels.Csv.Edit;
using DesktopApplicationTemplate.Services.Csv.UI.Views.Csv;
using DesktopApplicationTemplate.Services.Csv.UI.Views.Csv.Advanced;
using DesktopApplicationTemplate.Services.Csv.UI.Views.Csv.Edit;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Services.Csv.UI.Modules;

/// <summary>
/// Registers CSV UI components with the application host.
/// </summary>
public sealed class CsvUiModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<CsvViewerViewModel>();
        services.AddSingleton<ICsvService, CsvService>();

        services.AddTransient<CsvServiceFactory>();
        services.AddTransient<CsvNavigationHandler>();
        services.AddTransient<CsvEditServiceHandler>();

        services.AddTransient<CsvServiceEditorViewModel>();
        services.AddTransient<CsvAdvancedConfigViewModel>();

        services.AddTransient<CsvServiceEditorView>();
        services.AddTransient<CsvAdvancedConfigView>();
        services.AddTransient<CsvServiceView>();
    }

    public IEnumerable<IServiceDescriptor> DescribeServices() => Array.Empty<IServiceDescriptor>();
}
