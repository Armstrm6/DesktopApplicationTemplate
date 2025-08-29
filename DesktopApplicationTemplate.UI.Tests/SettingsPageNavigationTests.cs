using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Models;
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
        [WindowsFact]
        public void NavigateBack_ReturnsToHomePage()
        {
            ApplicationResourceHelper.RunOnDispatcher(() =>
            {
                var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
                var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
                Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);

                var networkSvc = new Mock<INetworkConfigurationService>();
                networkSvc.Setup(s => s.GetConfigurationAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new NetworkConfiguration());
                var networkVm = new NetworkConfigurationViewModel(networkSvc.Object);

                var mainVm = new MainViewModel(new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath)), networkVm, networkSvc.Object, null, servicesPath);
                var mainView = new MainView(mainVm);
                var settingsPage = new SettingsPage(new SettingsViewModel(), networkVm);
                mainView.ContentFrame.Content = settingsPage;

                settingsPage.NavigateBack();

                Assert.Null(mainView.ContentFrame.Content);
                ConsoleTestLogger.LogPass();
            });
        }
    }
}
