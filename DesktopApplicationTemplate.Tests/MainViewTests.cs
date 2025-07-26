using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Services;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;
using System.IO;

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
                    var vm = new MainViewModel(new CsvService(new CsvViewerViewModel(configPath)), null, servicesPath);
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
                    var vm = new MainViewModel(new CsvService(new CsvViewerViewModel(configPath)), null, servicesPath);
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
    }
}
