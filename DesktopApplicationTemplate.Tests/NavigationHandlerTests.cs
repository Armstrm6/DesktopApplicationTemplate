using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.UI.Navigation;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Http.Create;
using DesktopApplicationTemplate.UI.ViewModels.Http.Advanced;
using DesktopApplicationTemplate.UI.Views;
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
        var handler = provider.GetKeyedService<INavigationHandler>(ServiceType.Http)!;
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
        var handler = provider.GetKeyedService<INavigationHandler>(ServiceType.Http)!;
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
        var handler = provider.GetKeyedService<INavigationHandler>(ServiceType.Http)!;
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
}
