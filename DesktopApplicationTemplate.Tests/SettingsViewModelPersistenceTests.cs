using System;
using System.IO;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Tests
{
    [Collection("NonParallel")]
    public class SettingsViewModelPersistenceTests
    {
        [Fact]
        public void SaveAndLoad_PersistsFirstRunAndSuppression()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var original = SettingsViewModel.FilePath;
            var suppressOrig = SettingsViewModel.SaveConfirmationSuppressed;
            var closeOrig = SettingsViewModel.CloseConfirmationSuppressed;
            SettingsViewModel.FilePath = Path.Combine(tempDir, "userSettings.json");
            try
            {
                var vm = new SettingsViewModel { FirstRun = false };
                SettingsViewModel.SaveConfirmationSuppressed = true;
                SettingsViewModel.CloseConfirmationSuppressed = true;
                vm.Save();

                var vm2 = new SettingsViewModel { FirstRun = true };
                vm2.Load();

                Assert.False(vm2.FirstRun);
                Assert.True(SettingsViewModel.SaveConfirmationSuppressed);
                Assert.True(SettingsViewModel.CloseConfirmationSuppressed);
            }
            finally
            {
                SettingsViewModel.FilePath = original;
                SettingsViewModel.SaveConfirmationSuppressed = suppressOrig;
                SettingsViewModel.CloseConfirmationSuppressed = closeOrig;
                Directory.Delete(tempDir, true);
            }

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public void SaveAndLoad_PreservesPreferredServiceType()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var original = SettingsViewModel.FilePath;
            SettingsViewModel.FilePath = Path.Combine(tempDir, "userSettings.json");
            try
            {
                var vm = new SettingsViewModel { PreferredServiceType = ServiceType.Mqtt };
                vm.Save();

                var json = File.ReadAllText(SettingsViewModel.FilePath);
                Assert.Contains(ServiceType.Mqtt.ToCode(), json);

                var vm2 = new SettingsViewModel();
                vm2.Load();
                Assert.Equal(ServiceType.Mqtt, vm2.PreferredServiceType);
            }
            finally
            {
                SettingsViewModel.FilePath = original;
                Directory.Delete(tempDir, true);
            }

            ConsoleTestLogger.LogPass();
        }
    }
}
