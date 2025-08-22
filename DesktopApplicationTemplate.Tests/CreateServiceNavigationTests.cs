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
        });
        windowThread.SetApartmentState(ApartmentState.STA);
        windowThread.Start();
        windowThread.Join();
    }
}
