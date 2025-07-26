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
                    var app = System.Windows.Application.Current ?? new DesktopApplicationTemplate.UI.App();
                    bool created = System.Windows.Application.Current == null;
                    var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    var vm = new MainViewModel(new CsvService(new CsvViewerViewModel(configPath)));
                    var view = new MainView(vm);
                    var list = view.FindName("ServiceList") as System.Windows.Controls.ListBox;
                    Assert.Equal(350, list?.MaxHeight);
                    if (created) app.Shutdown();
                }
                catch (Exception e) { ex = e; }
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
                    var app = System.Windows.Application.Current ?? new DesktopApplicationTemplate.UI.App();
                    bool created = System.Windows.Application.Current == null;
                    var configPath2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    var vm = new MainViewModel(new CsvService(new CsvViewerViewModel(configPath2)));
                    var view = new MainView(vm);
                    bool bound = view.CommandBindings.OfType<CommandBinding>()
                                        .Any(b => b.Command == SystemCommands.CloseWindowCommand);
                    Assert.True(bound);
                    if (created) app.Shutdown();
                }
                catch (Exception e) { ex = e; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;
            ConsoleTestLogger.LogPass();
        }
    }
}
