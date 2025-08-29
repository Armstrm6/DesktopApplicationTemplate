using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xunit;
using System.IO;
using Moq;

namespace DesktopApplicationTemplate.Tests
{
    public class MainViewTests
    {
        [WindowsFact]
        public void MainView_ServiceList_HasMaxHeight()
        {
            ApplicationResourceHelper.RunOnDispatcher(() =>
            {
                var configPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString() + ".json");

                var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
                Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
                var network = new Mock<INetworkConfigurationService>();
                var networkVm = new NetworkConfigurationViewModel(network.Object);
                var vm = new MainViewModel(new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath)), networkVm, network.Object, null, servicesPath);
                var view = new MainView(vm);
                var list = view.FindName("ServiceList") as System.Windows.Controls.ListBox;
                Assert.Equal(350, list?.MaxHeight);
                ConsoleTestLogger.LogPass();
            });
        }

        [WindowsFact]
        public void MainView_HasCloseCommandBinding()
        {
            ApplicationResourceHelper.RunOnDispatcher(() =>
            {
                var configPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString() + ".json");
                var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
                Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
                var network = new Mock<INetworkConfigurationService>();
                var networkVm = new NetworkConfigurationViewModel(network.Object);
                var vm = new MainViewModel(new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath)), networkVm, network.Object, null, servicesPath);
                var view = new MainView(vm);
                bool bound = view.CommandBindings.OfType<CommandBinding>()
                                    .Any(b => b.Command == SystemCommands.CloseWindowCommand);
                Assert.True(bound);
                ConsoleTestLogger.LogPass();
            });
        }

        [WindowsFact]
        public void MainView_HasMinimizeCommandBinding()
        {
            ApplicationResourceHelper.RunOnDispatcher(() =>
            {
                var configPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString() + ".json");
                var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
                Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
                var network = new Mock<INetworkConfigurationService>();
                var networkVm = new NetworkConfigurationViewModel(network.Object);
                var vm = new MainViewModel(new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath)), networkVm, network.Object, null, servicesPath);
                var view = new MainView(vm);
                bool bound = view.CommandBindings.OfType<CommandBinding>()
                                    .Any(b => b.Command == SystemCommands.MinimizeWindowCommand);
                Assert.True(bound);
                ConsoleTestLogger.LogPass();
            });
        }

        [WindowsFact]
        public void ShowService_NonCsv_SetsContentFrame()
        {
            ApplicationResourceHelper.RunOnDispatcher(() =>
            {
                var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
                var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
                Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
                var network = new Mock<INetworkConfigurationService>();
                var networkVm = new NetworkConfigurationViewModel(network.Object);
                var vm = new MainViewModel(new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath)), networkVm, network.Object, null, servicesPath);
                var view = new MainView(vm);
                var svc = new ServiceViewModel { DisplayName = "TCP - Test", ServiceType = "TCP" };
                svc.SetColorsByType();
                var getPage = typeof(MainView).GetMethod("GetOrCreateServicePage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var page = (Page?)getPage?.Invoke(view, new object[] { svc });
                var showPage = typeof(MainView).GetMethod("ShowPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                showPage?.Invoke(view, new object[] { page! });
                Assert.NotNull(view.ContentFrame.Content);
                ConsoleTestLogger.LogPass();
            });
        }

        [WindowsFact]
        public void ShowService_CsvCreator_LoadsView()
        {
            ApplicationResourceHelper.RunOnDispatcher(() =>
            {
                var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
                var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
                Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
                var network = new Mock<INetworkConfigurationService>();
                var networkVm = new NetworkConfigurationViewModel(network.Object);
                var csvVm = new CsvViewerViewModel(new StubFileDialogService(), configPath);
                var vm = new MainViewModel(new CsvService(csvVm), networkVm, network.Object, null, servicesPath);
                var view = new MainView(vm);
                var svc = new ServiceViewModel { DisplayName = "CSV - Test", ServiceType = "CSV Creator" };
                svc.SetColorsByType();
                var getPage = typeof(MainView).GetMethod("GetOrCreateServicePage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var page = (Page?)getPage?.Invoke(view, new object[] { svc });
                var showPage = typeof(MainView).GetMethod("ShowPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                showPage?.Invoke(view, new object[] { page! });
                Assert.IsType<CsvServiceView>(view.ContentFrame.Content);
                ConsoleTestLogger.LogPass();
            });
        }

        [WindowsFact]
        public void MainView_KeyDown_IgnoresNonEscape()
        {
            ApplicationResourceHelper.RunOnDispatcher(() =>
            {
                var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
                var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
                Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
                var network = new Mock<INetworkConfigurationService>();
                var networkVm = new NetworkConfigurationViewModel(network.Object);
                var vm = new MainViewModel(new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath)), networkVm, network.Object, null, servicesPath);
                vm.SelectedService = new ServiceViewModel { DisplayName = "Svc", ServiceType = "TCP" };
                var view = new MainView(vm);

                var method = typeof(MainView).GetMethod("MainView_KeyDown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var src = new TestPresentationSource();

                var dArgs = new KeyEventArgs(Keyboard.PrimaryDevice, src, 0, Key.D) { RoutedEvent = Keyboard.KeyDownEvent };
                method?.Invoke(view, new object[] { view, dArgs });
                Assert.NotNull(vm.SelectedService);

                var escArgs = new KeyEventArgs(Keyboard.PrimaryDevice, src, 0, Key.Escape) { RoutedEvent = Keyboard.KeyDownEvent };
                method?.Invoke(view, new object[] { view, escArgs });
                Assert.Null(vm.SelectedService);
                ConsoleTestLogger.LogPass();
            });
        }

        [WindowsFact]
        public void HeaderBar_DoubleClick_TogglesWindowState()
        {
            ApplicationResourceHelper.RunOnDispatcher(() =>
            {
                var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
                var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
                Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
                var network = new Mock<INetworkConfigurationService>();
                var networkVm = new NetworkConfigurationViewModel(network.Object);
                var vm = new MainViewModel(new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath)), networkVm, network.Object, null, servicesPath);
                var view = new MainView(vm) { WindowState = WindowState.Normal };
                var method = typeof(MainView).GetMethod("HeaderBar_MouseDoubleClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Control.MouseDoubleClickEvent };

                method?.Invoke(view, new object[] { view, args });
                Assert.Equal(WindowState.Maximized, view.WindowState);

                method?.Invoke(view, new object[] { view, args });
                Assert.Equal(WindowState.Normal, view.WindowState);
                ConsoleTestLogger.LogPass();
            });
        }

        private class TestPresentationSource : PresentationSource
        {
            public override Visual RootVisual { get; set; } = new DrawingVisual();
            protected override CompositionTarget? GetCompositionTargetCore() => null;
            public override bool IsDisposed => false;
        }
    }
}
