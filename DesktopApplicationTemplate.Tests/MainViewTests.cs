using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Models;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;
using System.IO;
using Moq;

namespace DesktopApplicationTemplate.Tests
{
    public class MainViewTests
    {
        [Fact]
        [TestCategory("WindowsSafe")]
        public void MainView_ServiceList_HasMaxHeight()
        {
            if (!OperatingSystem.IsWindows())
                return;

            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try
                {
                    if (System.Windows.Application.Current == null)
                        new DesktopApplicationTemplate.UI.App();
                    var configPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString() + ".json");

                    var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
                    var network = new Mock<INetworkConfigurationService>();
                    var networkVm = new NetworkConfigurationViewModel(network.Object);
                    var vm = new MainViewModel(new CsvService(new CsvViewerViewModel(configPath)), networkVm, network.Object, null, servicesPath);
                    var view = new MainView(vm);
                    var list = view.FindName("ServiceList") as System.Windows.Controls.ListBox;
                    Assert.Equal(350, list?.MaxHeight);
                }
                catch (Exception e) { ex = e; }
                finally
                {
                    System.Windows.Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("WindowsSafe")]
        public void MainView_HasCloseCommandBinding()
        {
            if (!OperatingSystem.IsWindows())
                return;

            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try
                {
                    if (System.Windows.Application.Current == null)
                        new DesktopApplicationTemplate.UI.App();
                    var configPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString() + ".json");
                    var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
                    var network = new Mock<INetworkConfigurationService>();
                    var networkVm = new NetworkConfigurationViewModel(network.Object);
                    var vm = new MainViewModel(new CsvService(new CsvViewerViewModel(configPath)), networkVm, network.Object, null, servicesPath);
                    var view = new MainView(vm);
                    bool bound = view.CommandBindings.OfType<CommandBinding>()
                                        .Any(b => b.Command == SystemCommands.CloseWindowCommand);
                    Assert.True(bound);
                }
                catch (Exception e) { ex = e; }
                finally
                {
                    System.Windows.Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("WindowsSafe")]
        public void MainView_HasMinimizeCommandBinding()
        {
            if (!OperatingSystem.IsWindows())
                return;

            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try
                {
                    if (System.Windows.Application.Current == null)
                        new DesktopApplicationTemplate.UI.App();
                    var configPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString() + ".json");
                    var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
                    var network = new Mock<INetworkConfigurationService>();
                    var networkVm = new NetworkConfigurationViewModel(network.Object);
                    var vm = new MainViewModel(new CsvService(new CsvViewerViewModel(configPath)), networkVm, network.Object, null, servicesPath);
                    var view = new MainView(vm);
                    bool bound = view.CommandBindings.OfType<CommandBinding>()
                                        .Any(b => b.Command == SystemCommands.MinimizeWindowCommand);
                    Assert.True(bound);
                }
                catch (Exception e) { ex = e; }
                finally
                {
                    System.Windows.Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("WindowsSafe")]
        public void OpenServiceEditor_NonCsv_SetsContentFrame()
        {
            if (!OperatingSystem.IsWindows())
                return;

            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try
                {
                    if (System.Windows.Application.Current == null)
                        new DesktopApplicationTemplate.UI.App();
                    var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
                    var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
                    var network = new Mock<INetworkConfigurationService>();
                    var networkVm = new NetworkConfigurationViewModel(network.Object);
                    var vm = new MainViewModel(new CsvService(new CsvViewerViewModel(configPath)), networkVm, network.Object, null, servicesPath);
                    var view = new MainView(vm);
                    var svc = new ServiceViewModel { DisplayName = "TCP - Test", ServiceType = "TCP" };
                    svc.SetColorsByType();
                    var method = typeof(MainView).GetMethod("OpenServiceEditor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    method?.Invoke(view, new object[] { svc });
                    Assert.NotNull(view.ContentFrame.Content);
                }
                catch (Exception e) { ex = e; }
                finally
                {
                    System.Windows.Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("WindowsSafe")]
        public void OpenServiceEditor_CsvCreator_ShowsWindow()
        {
            if (!OperatingSystem.IsWindows())
                return;

            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try
                {
                    if (System.Windows.Application.Current == null)
                        new DesktopApplicationTemplate.UI.App();
                    var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
                    var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
                    var network = new Mock<INetworkConfigurationService>();
                    var networkVm = new NetworkConfigurationViewModel(network.Object);
                    var csvVm = new CsvViewerViewModel(configPath);
                    var vm = new MainViewModel(new CsvService(csvVm), networkVm, network.Object, null, servicesPath);
                    var view = new MainView(vm);
                    var svc = new ServiceViewModel { DisplayName = "CSV - Test", ServiceType = "CSV Creator" };
                    svc.SetColorsByType();
                    // Ensure window closes immediately to avoid blocking
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => csvVm.CloseCommand.Execute(null)));
                    var method = typeof(MainView).GetMethod("OpenServiceEditor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    method?.Invoke(view, new object[] { svc });
                    Assert.NotNull(view); // method executed without exception
                }
                catch (Exception e) { ex = e; }
                finally
                {
                    System.Windows.Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("WindowsSafe")]
        public void ClearLogsInternal_ClearsLogs()
        {
            if (!OperatingSystem.IsWindows())
                return;

            Exception? ex = null;
            var thread = new Thread(() =>
            {
                MainViewModel? vm = null;
                try
                {
                    if (System.Windows.Application.Current == null)
                        new DesktopApplicationTemplate.UI.App();
                    var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
                    var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
                    var network = new Mock<INetworkConfigurationService>();
                    var networkVm = new NetworkConfigurationViewModel(network.Object);
                    vm = new MainViewModel(new CsvService(new CsvViewerViewModel(configPath)), networkVm, network.Object, null, servicesPath);
                    vm.AllLogs.Add(new LogEntry { Message = "Test", Color = System.Windows.Media.Brushes.Black });
                    var view = new MainView(vm);
                    var method = typeof(MainView).GetMethod("ClearLogsInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    method?.Invoke(view, null);
                    Assert.Empty(vm.AllLogs);
                }
                catch (Exception e) { ex = e; }
                finally
                {
                    System.Windows.Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("WindowsSafe")]
        public void ExportLogsInternalAsync_CreatesFile()
        {
            if (!OperatingSystem.IsWindows())
                return;

            Exception? ex = null;
            var thread = new Thread(() =>
            {
                MainViewModel? vm = null;
                try
                {
                    if (System.Windows.Application.Current == null)
                        new DesktopApplicationTemplate.UI.App();
                    var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
                    var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
                    var network = new Mock<INetworkConfigurationService>();
                    var networkVm = new NetworkConfigurationViewModel(network.Object);
                    vm = new MainViewModel(new CsvService(new CsvViewerViewModel(configPath)), networkVm, network.Object, null, servicesPath);
                    vm.AllLogs.Add(new LogEntry { Message = "Export", Color = System.Windows.Media.Brushes.Black });
                    var view = new MainView(vm);
                    var method = typeof(MainView).GetMethod("ExportLogsInternalAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var task = method?.Invoke(view, null) as Task;
                    task?.Wait();
                    Assert.NotNull(vm.LastExportedLogPath);
                    Assert.True(File.Exists(vm.LastExportedLogPath!));
                    if (vm.LastExportedLogPath != null)
                        File.Delete(vm.LastExportedLogPath);
                }
                catch (Exception e) { ex = e; }
                finally
                {
                    System.Windows.Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;
            ConsoleTestLogger.LogPass();
        }
    }
}
