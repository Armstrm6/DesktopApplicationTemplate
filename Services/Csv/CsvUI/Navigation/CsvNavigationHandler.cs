using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Csv.Descriptors;
using DesktopApplicationTemplate.Services.Csv.UI.ViewModels.Csv.Advanced;
using DesktopApplicationTemplate.Services.Csv.UI.ViewModels.Csv.Edit;
using DesktopApplicationTemplate.Services.Csv.UI.Views.Csv.Advanced;
using DesktopApplicationTemplate.Services.Csv.UI.Views.Csv.Edit;
using DesktopApplicationTemplate.UI.Configuration;
using DesktopApplicationTemplate.UI.Factories;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Services.Csv.UI.Navigation;

[ServiceDescriptorRegistration(CsvServiceDescriptor.DescriptorId, ServiceRegistrationKind.NavigationHandler)]
public class CsvNavigationHandler : INavigationHandler
{
    private readonly IServiceProvider _services;
    private readonly Func<MainView> _getMainView;
    private readonly IServiceCatalog _catalog;

    public string DescriptorId => CsvServiceDescriptor.DescriptorId;

    public CsvNavigationHandler(IServiceProvider services, Func<MainView> getMainView, IServiceCatalog catalog)
    {
        _services = services;
        _getMainView = getMainView;
        _catalog = catalog;
    }

    public Page CreateView(string defaultName)
    {
        var viewModel = _services.GetRequiredService<CsvServiceEditorViewModel>();
        viewModel.ServiceName = defaultName;
        var mainView = _getMainView();
        viewModel.ServiceSaved += (name, options) => _ = AddServiceAsync(name, options);
        viewModel.EditCancelled += mainView.ShowCreateServiceSelectionPage;
        var view = _services.GetRequiredService<CsvServiceEditorView>();
        view.Initialize(viewModel);
        viewModel.AdvancedConfigRequested += options =>
        {
            var advancedViewModel = ActivatorUtilities.CreateInstance<CsvAdvancedConfigViewModel>(_services, options);
            var advancedView = _services.GetRequiredService<CsvAdvancedConfigView>();
            advancedView.Initialize(advancedViewModel);
            advancedViewModel.Saved += _ => mainView.ShowPage(view);
            advancedViewModel.BackRequested += () => mainView.ShowPage(view);
            mainView.ShowPage(advancedView);
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
