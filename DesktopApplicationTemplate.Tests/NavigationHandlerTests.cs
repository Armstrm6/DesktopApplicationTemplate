using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.UI.Navigation;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Http.Create;
using DesktopApplicationTemplate.UI.ViewModels.Http.Advanced;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class NavigationHandlerTests
{
    static NavigationHandlerTests()
    {
        _ = new App();
    }

    [WindowsFact]
    public async Task CreateView_ServiceSaved_AddsService()
    {
        var provider = App.AppHost.Services;
        var mainView = provider.GetRequiredService<MainView>();
        var catalog = provider.GetRequiredService<IServiceCatalog>();
        var registry = provider.GetRequiredService<IServiceUiRegistry>();
        var descriptorId = catalog.LegacyMap[ServiceType.Http];
        var handler = registry.NavigationHandlers[descriptorId]();
        var createPage = handler.CreateView("svc");
        var vm = (HttpCreateServiceViewModel)createPage.DataContext!;

        var mainVm = (MainViewModel)typeof(MainView).GetField("_viewModel", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(mainView)!;
        mainVm.Services.Clear();

        vm.ServiceName = "svc";
        vm.BaseUrl = "http://example";
        vm.CreateCommand.Execute(null);
        await Task.Delay(10);

        Assert.Single(mainVm.Services);
    }

    [WindowsFact]
    public async Task AddServiceAsync_AddsService()
    {
        var provider = App.AppHost.Services;
        var mainView = provider.GetRequiredService<MainView>();
        var catalog = provider.GetRequiredService<IServiceCatalog>();
        var registry = provider.GetRequiredService<IServiceUiRegistry>();
        var descriptorId = catalog.LegacyMap[ServiceType.Http];
        var handler = registry.NavigationHandlers[descriptorId]();
        var mainVm = (MainViewModel)typeof(MainView).GetField("_viewModel", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(mainView)!;
        mainVm.Services.Clear();

        await handler.AddServiceAsync("svc", new HttpServiceOptions { BaseUrl = "http://example" });

        Assert.Single(mainVm.Services);
    }

    [WindowsFact]
    public void AdvancedConfig_ReturnsToCreateView()
    {
        var provider = App.AppHost.Services;
        var mainView = provider.GetRequiredService<MainView>();
        var catalog = provider.GetRequiredService<IServiceCatalog>();
        var registry = provider.GetRequiredService<IServiceUiRegistry>();
        var descriptorId = catalog.LegacyMap[ServiceType.Http];
        var handler = registry.NavigationHandlers[descriptorId]();
        var createPage = handler.CreateView("svc");
        mainView.ShowPage(createPage);
        var vm = (HttpCreateServiceViewModel)createPage.DataContext!;
        var frame = (Frame)typeof(MainView).GetField("ContentFrame", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(mainView)!;

        vm.AdvancedConfigCommand.Execute(new HttpServiceOptions());
        var advPage = (Page)frame.Content!;
        Assert.NotSame(createPage, advPage);
        var advVm = (HttpAdvancedConfigViewModel)advPage.DataContext!;
        advVm.BackCommand.Execute(null);
        Assert.Same(createPage, frame.Content);

        vm.AdvancedConfigCommand.Execute(new HttpServiceOptions());
        advPage = (Page)frame.Content!;
        advVm = (HttpAdvancedConfigViewModel)advPage.DataContext!;
        advVm.SaveCommand.Execute(null);
        Assert.Same(createPage, frame.Content);
    }

    [WindowsFact]
    public void CatalogDescriptorsProvideNavigationHandlers()
    {
        var provider = App.AppHost.Services;
        var catalog = provider.GetRequiredService<IServiceCatalog>();
        var registry = provider.GetRequiredService<IServiceUiRegistry>();

        foreach (var descriptor in catalog.Descriptors)
        {
            Assert.True(registry.NavigationHandlers.ContainsKey(descriptor.Id));
        }
    }
}
