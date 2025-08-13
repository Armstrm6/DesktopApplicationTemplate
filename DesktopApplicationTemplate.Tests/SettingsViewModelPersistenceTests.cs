using System;
using System.IO;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.Tests
{
    public class SettingsViewModelPersistenceTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
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
                var vm = new SettingsViewModel();
                vm.FirstRun = false;
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
    }
}
