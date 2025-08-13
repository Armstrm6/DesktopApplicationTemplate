using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Models;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class SettingsPageNavigationTests
    {
        [Fact]
        [TestCategory("WindowsSafe")]
        public void NavigateBack_ReturnsToHomePage()
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

                    var networkSvc = new Mock<INetworkConfigurationService>();
                    networkSvc.Setup(s => s.GetConfigurationAsync(It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new NetworkConfiguration());
                    var networkVm = new NetworkConfigurationViewModel(networkSvc.Object);

                    var mainVm = new MainViewModel(new CsvService(new CsvViewerViewModel(configPath)), networkVm, networkSvc.Object, null, servicesPath);
                    var mainView = new MainView(mainVm);
                    var settingsPage = new SettingsPage(new SettingsViewModel(), networkVm);
                    mainView.ContentFrame.Content = settingsPage;

                    settingsPage.NavigateBack();

                    Assert.Null(mainView.ContentFrame.Content);
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
