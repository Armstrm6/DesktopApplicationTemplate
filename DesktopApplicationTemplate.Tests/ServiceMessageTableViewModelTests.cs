using System;
using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class ServiceMessageTableViewModelTests
{
    [Fact]
    public void AddMessage_InsertsAtTopWithTimestamp()
    {
        var vm = new ServiceMessageTableViewModel();
        vm.AddMessage("in1", "out1", "dest1");
        vm.AddMessage("in2", "out2", "dest2");

        Assert.Equal("in2", vm.Messages[0].IncomingMessage);
        Assert.True((DateTime.Now - vm.Messages[0].Timestamp).TotalSeconds < 5);
    }

    [Fact]
    public void AddMessage_LimitsToFiveRows()
    {
        var vm = new ServiceMessageTableViewModel();
        for (int i = 0; i < 6; i++)
            vm.AddMessage($"in{i}", $"out{i}", $"dest{i}");

        Assert.Equal(5, vm.Messages.Count);
        Assert.Equal("in5", vm.Messages[0].IncomingMessage);
        Assert.Equal("in1", vm.Messages[^1].IncomingMessage);
    }
}
