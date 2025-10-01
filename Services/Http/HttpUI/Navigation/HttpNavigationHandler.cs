using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Http.Descriptors;
using DesktopApplicationTemplate.Services.Http.UI.ViewModels.Http.Advanced;
using DesktopApplicationTemplate.Services.Http.UI.ViewModels.Http.Create;
using DesktopApplicationTemplate.Services.Http.UI.Views.Http.Advanced;
using DesktopApplicationTemplate.Services.Http.UI.Views.Http.Create;
using DesktopApplicationTemplate.UI.Configuration;
using DesktopApplicationTemplate.UI.Navigation;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Services.Http.UI.Navigation;

[ServiceDescriptorRegistration(HttpServiceDescriptor.DescriptorId, ServiceRegistrationKind.NavigationHandler)]
public class HttpNavigationHandler : INavigationHandler
{
    private readonly IServiceProvider _services;
    private readonly Func<MainView> _getMainView;
    private readonly IServiceCatalog _catalog;

    public string DescriptorId => HttpServiceDescriptor.DescriptorId;

    public HttpNavigationHandler(IServiceProvider services, Func<MainView> getMainView, IServiceCatalog catalog)
    {
        _services = services;
        _getMainView = getMainView;
        _catalog = catalog;
    }

    public Page CreateView(string defaultName)
    {
        var vm = _services.GetRequiredService<HttpCreateServiceViewModel>();
        vm.ServiceName = defaultName;
        var mainView = _getMainView();
        vm.ServiceSaved += (name, options) => _ = AddServiceAsync(name, options);
        vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
        var view = ActivatorUtilities.CreateInstance<HttpCreateServiceView>(_services, vm);
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<HttpAdvancedConfigViewModel>(_services, opts);
            var advView = _services.GetRequiredService<HttpAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => mainView.ShowPage(view);
            advVm.BackRequested += () => mainView.ShowPage(view);
            mainView.ShowPage(advView);
        };
        return view;
    }

    public Task AddServiceAsync(string name, object options)
    {
        var mainView = _getMainView();
        _catalog.TryGetById(DescriptorId, out var descriptor);
        var context = new ServiceFactoryContext(DescriptorId, name, options, descriptor);
        return mainView.AddServiceAsync(context);
    }
}
