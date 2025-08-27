using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Service.Services;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class ServiceScreenTests
{
    [Fact]
    public void Save_RaisesSavedEvent()
    {
        var logger = new Mock<ILoggingService>();
        var screen = new ServiceScreen<object>(logger.Object);
        string? name = null;
        object? opts = null;
        screen.Saved += (n, o) => { name = n; opts = o; };

        var options = new object();
        screen.Save("svc", options);

        Assert.Equal("svc", name);
        Assert.Same(options, opts);
    }

    [Fact]
    public void Cancel_RaisesCancelledEvent()
    {
        var screen = new ServiceScreen<object>();
        var cancelled = false;
        screen.Cancelled += () => cancelled = true;

        screen.Cancel();

        Assert.True(cancelled);
    }

    [Fact]
    public void OpenAdvanced_RaisesEvent()
    {
        var screen = new ServiceScreen<string>();
        string? received = null;
        screen.AdvancedConfigRequested += o => received = o;

        screen.OpenAdvanced("a");

        Assert.Equal("a", received);
    }
}

