using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.Tests.Services;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class ComposedTestServiceTests
{
    [Fact]
    public void Save_ValidInput_RaisesServiceSaved()
    {
        var rule = new ServiceRule();
        var screen = new ServiceScreen<TestServiceOptions>();
        string? capturedName = null;
        TestServiceOptions? capturedOptions = null;
        screen.ServiceSaved += (name, options) =>
        {
            capturedName = name;
            capturedOptions = options;
        };

        var service = new ComposedTestService(rule, screen);
        var options = new TestServiceOptions(1234);

        var result = service.Save("Test", options);

        Assert.True(result);
        Assert.Null(service.LastError);
        Assert.Equal("Test", capturedName);
        Assert.Equal(options, capturedOptions);
    }

    [Fact]
    public void Save_InvalidName_ReturnsFalseAndDoesNotRaiseEvent()
    {
        var rule = new ServiceRule();
        var screen = new ServiceScreen<TestServiceOptions>();
        var service = new ComposedTestService(rule, screen);
        bool raised = false;
        screen.ServiceSaved += (_, _) => raised = true;

        var result = service.Save(string.Empty, new TestServiceOptions(10));

        Assert.False(result);
        Assert.NotNull(service.LastError);
        Assert.False(raised);
    }
}

