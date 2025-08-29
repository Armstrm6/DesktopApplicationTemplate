using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using FluentAssertions;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class CreateServicePageTests
{
    [WindowsFact]
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

    [WindowsFact]
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

    [WindowsFact]
    public void ServiceType_Click_RaisesHeartbeatSelected()
    {
        string? receivedName = null;
        var thread = new Thread(() =>
        {
            var vm = new CreateServiceViewModel();
            var page = new CreateServicePage(vm);
            page.HeartbeatSelected += name => receivedName = name;
            var button = new Button { DataContext = new CreateServiceViewModel.ServiceTypeMetadata("Heartbeat", "Heartbeat", string.Empty) };
            var method = typeof(CreateServicePage).GetMethod("ServiceType_Click", BindingFlags.Instance | BindingFlags.NonPublic)!;
            method.Invoke(page, new object[] { button, new RoutedEventArgs() });
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        receivedName.Should().Be("Heartbeat1");
    }

    [WindowsFact]
    public void ServiceType_Click_RaisesHidSelected()
    {
        string? receivedName = null;
        var thread = new Thread(() =>
        {
            var vm = new CreateServiceViewModel();
            var page = new CreateServicePage(vm);
            page.HidSelected += name => receivedName = name;
            var button = new Button { DataContext = new CreateServiceViewModel.ServiceTypeMetadata("HID", "HID", string.Empty) };
            var method = typeof(CreateServicePage).GetMethod("ServiceType_Click", BindingFlags.Instance | BindingFlags.NonPublic)!;
            method.Invoke(page, new object[] { button, new RoutedEventArgs() });
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        receivedName.Should().Be("HID1");
    }

    [WindowsFact]
    public void ServiceType_Click_RaisesCsvSelected()
    {
        string? receivedName = null;
        var thread = new Thread(() =>
        {
            var vm = new CreateServiceViewModel();
            var page = new CreateServicePage(vm);
            page.CsvSelected += name => receivedName = name;
            var button = new Button { DataContext = new CreateServiceViewModel.ServiceTypeMetadata("CSV Creator", "CSV Creator", string.Empty) };
            var method = typeof(CreateServicePage).GetMethod("ServiceType_Click", BindingFlags.Instance | BindingFlags.NonPublic)!;
            method.Invoke(page, new object[] { button, new RoutedEventArgs() });
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        receivedName.Should().Be("CSV Creator1");
    }

    [WindowsFact]
    public void ServiceType_Click_RaisesHttpSelected()
    {
        string? receivedName = null;
        var thread = new Thread(() =>
        {
            var vm = new CreateServiceViewModel();
            var page = new CreateServicePage(vm);
            page.HttpSelected += name => receivedName = name;
            var button = new Button { DataContext = new CreateServiceViewModel.ServiceTypeMetadata("HTTP", "HTTP", string.Empty) };
            var method = typeof(CreateServicePage).GetMethod("ServiceType_Click", BindingFlags.Instance | BindingFlags.NonPublic)!;
            method.Invoke(page, new object[] { button, new RoutedEventArgs() });
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        receivedName.Should().Be("HTTP1");
    }

    [WindowsFact]
    public void Cancel_Click_RaisesCancelled()
    {
        bool cancelled = false;
        var thread = new Thread(() =>
        {
            var vm = new CreateServiceViewModel();
            var page = new CreateServicePage(vm);
            page.Cancelled += () => cancelled = true;
            var method = typeof(CreateServicePage).GetMethod("Cancel_Click", BindingFlags.Instance | BindingFlags.NonPublic)!;
            method.Invoke(page, new object[] { new Button(), new RoutedEventArgs() });
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        cancelled.Should().BeTrue();
    }
}
