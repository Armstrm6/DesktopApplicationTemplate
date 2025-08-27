using System;
using System.IO;
using DesktopApplicationTemplate.UI.ViewModels;
using FluentAssertions;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class CsvViewerViewModelTests
{
    [Fact]
    public void Save_DoesNotThrow_WhenConfigurationIsEmpty()
    {
        var temp = Path.GetTempFileName();
        try
        {
            var vm = new CsvViewerViewModel(new StubFileDialogService(), temp);
            vm.Configuration.Columns.Clear();
            Action act = () => vm.Save();
            act.Should().NotThrow();
        }
        finally
        {
            if (File.Exists(temp))
            {
                File.Delete(temp);
            }
        }
        }

#if DEBUG
    [Fact]
    public void DebugSaveCommand_WritesConfiguration()
    {
        var temp = Path.GetTempFileName();
        try
        {
            var vm = new CsvViewerViewModel(new StubFileDialogService(), temp);
            vm.Configuration.Columns.Clear();
            vm.DebugSaveCommand.Execute(null);
            File.Exists(temp).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(temp))
                File.Delete(temp);
        }
        }
#endif

    [Fact]
    public void BrowseCommand_SetsOutputDirectory()
    {
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var vm = new CsvViewerViewModel(new StubFileDialogService(folderPath: temp));
        vm.BrowseCommand.Execute(null);
        vm.Configuration.OutputDirectory.Should().Be(temp);
    }
}
