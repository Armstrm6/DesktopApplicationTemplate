using System;
using System.IO;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.Tests
{
    public class SettingsViewModelPersistenceTests
    {
        [Fact]
        public void SaveAndLoad_PersistsFirstRun()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var original = SettingsViewModel.FilePath;
            SettingsViewModel.FilePath = Path.Combine(tempDir, "userSettings.json");
            try
            {
                var vm = new SettingsViewModel();
                vm.FirstRun = false;
                vm.Save();

                var vm2 = new SettingsViewModel { FirstRun = true };
                vm2.Load();

                Assert.False(vm2.FirstRun);
            }
            finally
            {
                SettingsViewModel.FilePath = original;
                Directory.Delete(tempDir, true);
            }
        }
    }
}
