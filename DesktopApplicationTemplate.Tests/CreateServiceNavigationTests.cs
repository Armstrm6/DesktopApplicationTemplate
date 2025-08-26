using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class CreateServiceNavigationTests
{
    [Fact]
    public void ServiceType_Click_RaisesTcpSelected()
    {
        string? receivedName = null;
        var thread = new Thread(() =>
        {
            var vm = new CreateServiceViewModel();
            var page = new CreateServicePage(vm);
            page.TcpSelected += name => receivedName = name;
            var button = new Button { DataContext = new CreateServiceViewModel.ServiceTypeMetadata("TCP", "TCP", string.Empty) };
            var method = typeof(CreateServicePage).GetMethod("ServiceType_Click", BindingFlags.Instance | BindingFlags.NonPublic)!;
            method.Invoke(page, new object[] { button, new RoutedEventArgs() });
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        receivedName.Should().Be("TCP1");
    }

    [Fact]
    public void ServiceType_Click_RaisesFtpServerSelected()
    {
        string? receivedName = null;
        var thread = new Thread(() =>
        {
            var vm = new CreateServiceViewModel();
            var page = new CreateServicePage(vm);
            page.FtpServerSelected += name => receivedName = name;
            var button = new Button { DataContext = new CreateServiceViewModel.ServiceTypeMetadata("FTP Server", "FTP Server", string.Empty) };
            var method = typeof(CreateServicePage).GetMethod("ServiceType_Click", BindingFlags.Instance | BindingFlags.NonPublic)!;
            method.Invoke(page, new object[] { button, new RoutedEventArgs() });
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        receivedName.Should().Be("FTP Server1");
    }

    [Fact]
    public void NavigateToFtpServer_PopulatesOptionsAndClosesWindow()
    {
        var vm = new FtpServerCreateViewModel();
        var view = new FtpServerCreateView();
        var advView = new FtpServerAdvancedConfigView();
        var services = new ServiceCollection();
        services.AddSingleton(vm);
        services.AddSingleton(view);
        services.AddSingleton(advView);
        services.AddLogging();
        services.AddOptions<FtpServerOptions>().Configure(o =>
        {
            o.Port = 2121;
            o.RootPath = "/tmp";
            o.AllowAnonymous = true;
            o.Username = "user";
            o.Password = "pass";
        });
        var provider = services.BuildServiceProvider();

        var windowThread = new Thread(() =>
        {
            var logger = provider.GetRequiredService<ILogger<CreateServiceWindow>>();
            var window = new CreateServiceWindow(new CreateServiceViewModel(), provider, logger);
            var pageField = typeof(CreateServiceWindow).GetField("_page", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var page = (CreateServicePage)pageField.GetValue(window)!;
            var button = new Button { DataContext = new CreateServiceViewModel.ServiceTypeMetadata("FTP Server", "FTP Server", string.Empty) };
            var click = typeof(CreateServicePage).GetMethod("ServiceType_Click", BindingFlags.Instance | BindingFlags.NonPublic)!;
            click.Invoke(page, new object[] { button, new RoutedEventArgs() });

            vm.Options.Port.Should().Be(2121);
            vm.Options.RootPath.Should().Be("/tmp");
            vm.Options.AllowAnonymous.Should().BeTrue();
            vm.Options.Username.Should().Be("user");
            vm.Options.Password.Should().Be("pass");

            var evt = typeof(FtpServerCreateViewModel).GetField("ServerCreated", BindingFlags.Instance | BindingFlags.NonPublic);
            var del = (Action<string, FtpServerOptions>?)evt?.GetValue(vm);
            del?.Invoke("Svc", vm.Options);

            window.CreatedServiceType.Should().Be("FTP Server");
            window.CreatedServiceName.Should().Be("Svc");
            window.FtpServerOptions.Should().Be(vm.Options);
            window.DialogResult.Should().BeTrue();
            window.IsVisible.Should().BeFalse();
        });
        windowThread.SetApartmentState(ApartmentState.STA);
        windowThread.Start();
        windowThread.Join();
    }
    [Fact]
    public void NavigateToTcp_SetsPropertiesAndOptions()
    {
        var vm = new TcpCreateServiceViewModel();
        var view = new TcpCreateServiceView();
        var services = new ServiceCollection();
        services.AddSingleton(vm);
        services.AddSingleton(view);
        var provider = services.BuildServiceProvider();

        var windowThread = new Thread(() =>
        {
            var window = new CreateServiceWindow(new CreateServiceViewModel(), provider);
            var pageField = typeof(CreateServiceWindow).GetField("_page", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var page = (CreateServicePage)pageField.GetValue(window)!;
            var button = new Button { DataContext = new CreateServiceViewModel.ServiceTypeMetadata("TCP", "TCP", string.Empty) };
            var click = typeof(CreateServicePage).GetMethod("ServiceType_Click", BindingFlags.Instance | BindingFlags.NonPublic)!;
            click.Invoke(page, new object[] { button, new RoutedEventArgs() });

            var options = new TcpServiceOptions { Host = "h", Port = 1, Mode = TcpServiceMode.Listening };
            var evt = typeof(TcpCreateServiceViewModel).GetField("ServiceCreated", BindingFlags.Instance | BindingFlags.NonPublic);
            var del = (Action<string, TcpServiceOptions>?)evt?.GetValue(vm);
            del?.Invoke("Svc", options);

            window.CreatedServiceType.Should().Be("TCP");
            window.CreatedServiceName.Should().Be("Svc");
            window.TcpOptions.Should().Be(options);
            window.DialogResult.Should().BeTrue();
        });
        windowThread.SetApartmentState(ApartmentState.STA);
        windowThread.Start();
        windowThread.Join();
    }

    [Fact]
    public void ServiceCreated_ClosesWindow()
    {
        var services = new ServiceCollection().BuildServiceProvider();

        var thread = new Thread(() =>
        {
            var window = new CreateServiceWindow(new CreateServiceViewModel(), services);
            var pageField = typeof(CreateServiceWindow).GetField("_page", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var page = (CreateServicePage)pageField.GetValue(window)!;
            var button = new Button { DataContext = new CreateServiceViewModel.ServiceTypeMetadata("HTTP", "HTTP", string.Empty) };
            var click = typeof(CreateServicePage).GetMethod("ServiceType_Click", BindingFlags.Instance | BindingFlags.NonPublic)!;
            click.Invoke(page, new object[] { button, new RoutedEventArgs() });

            window.CreatedServiceType.Should().Be("HTTP");
            window.DialogResult.Should().BeTrue();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }

    [Fact]
    public void Cancel_Click_ClosesWindow()
    {
        var services = new ServiceCollection().BuildServiceProvider();

        var thread = new Thread(() =>
        {
            var window = new CreateServiceWindow(new CreateServiceViewModel(), services);
            var pageField = typeof(CreateServiceWindow).GetField("_page", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var page = (CreateServicePage)pageField.GetValue(window)!;
            var cancel = typeof(CreateServicePage).GetMethod("Cancel_Click", BindingFlags.Instance | BindingFlags.NonPublic)!;
            cancel.Invoke(page, new object[] { new Button(), new RoutedEventArgs() });

            window.DialogResult.Should().BeFalse();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
