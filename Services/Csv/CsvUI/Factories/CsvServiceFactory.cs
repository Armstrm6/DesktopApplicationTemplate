using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Csv.Options;
using DesktopApplicationTemplate.UI.Factories;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;

namespace DesktopApplicationTemplate.Services.Csv.UI.Factories;

public class CsvServiceFactory : IServiceFactory
{
    private readonly Func<MainView> _getMainView;
    private readonly IServiceCatalog _catalog;

    public ServiceType ServiceType => ServiceType.Csv;

    public CsvServiceFactory(Func<MainView> getMainView, IServiceCatalog catalog)
    {
        _getMainView = getMainView;
        _catalog = catalog;
    }

    public ServiceListModel Create(ServiceFactoryContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var descriptor = _catalog.TryGetById(context.DescriptorId, out var resolved)
            ? resolved
            : context.Descriptor;

        var options = context.GetPayload<CsvServiceOptions>() ?? new CsvServiceOptions();

        var service = new ServiceListModel
        {
            Type = ServiceType.Csv,
            DescriptorId = context.DescriptorId,
            IsActive = false
        };

        service.SetPayload(options);

        service.ApplyDescriptor(descriptor, context.ServiceName);

        _getMainView().GetOrCreateServicePage(service);

        return service;
    }
}
