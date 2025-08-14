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

    [Fact]
    public void DebugSaveCommand_WritesFile()
    {
        var temp = Path.GetTempFileName();
        try
        {
            var vm = new CsvViewerViewModel(temp);
            Action act = () => vm.DebugSaveCommand.Execute(null);
            act.Should().NotThrow();
            File.Exists(temp).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(temp))
            {
                File.Delete(temp);
            }
        }
    }
}
