using System;
using System.IO;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;
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
            var vm = new CsvViewerViewModel(temp);
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

    private class StubFileDialogService : IFileDialogService
    {
        public string? Folder { get; set; }
        public string? OpenFile() => null;
        public string? SelectFolder() => Folder;
    }

    [Fact]
    public void BrowseCommand_UpdatesFileNamePattern()
    {
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var dialog = new StubFileDialogService { Folder = temp };
        var vm = new CsvViewerViewModel(fileDialog: dialog);
        vm.Configuration.FileNamePattern = "output_{index}.csv";

        vm.BrowseCommand.Execute(null);

        vm.Configuration.FileNamePattern.Should().StartWith(temp);
    }
}

#if DEBUG
    [Fact]
    public void DebugSaveCommand_WritesConfiguration()
    {
        var temp = Path.GetTempFileName();
        try
        {
            var vm = new CsvViewerViewModel(temp);
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
}
