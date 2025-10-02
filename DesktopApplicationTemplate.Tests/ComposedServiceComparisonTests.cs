using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.Tests.Services;
using FluentAssertions;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class ComposedServiceComparisonTests
{
    [Fact]
    public void Save_ValidInput_MatchesOriginalService()
    {
        var originalLogger = new TestLogger();
        var original = new OriginalTestService(originalLogger);
        string? originalName = null;
        TestServiceOptions? originalOptions = null;
        original.ServiceSaved += (n, o) => { originalName = n; originalOptions = o; };

        var sampleLogger = new TestLogger();
        var screen = new ServiceScreen<TestServiceOptions>(sampleLogger);
        string? sampleName = null;
        TestServiceOptions? sampleOptions = null;
        screen.ServiceSaved += (n, o) => { sampleName = n; sampleOptions = o; };
        var sample = new ComposedTestService(new ServiceRule(), screen);

        var options = new TestServiceOptions(1234);

        var originalResult = original.Save("Test", options);
        var sampleResult = sample.Save("Test", options);

        Assert.True(originalResult);
        Assert.True(sampleResult);
        Assert.Null(original.LastError);
        Assert.Null(sample.LastError);
        Assert.Equal(originalName, sampleName);
        Assert.Equal(originalOptions, sampleOptions);
        originalLogger.Entries.Should().Equal(sampleLogger.Entries);
    }

    [Fact]
    public void Save_InvalidPort_MatchesOriginalService()
    {
        var originalLogger = new TestLogger();
        var original = new OriginalTestService(originalLogger);
        bool originalRaised = false;
        original.ServiceSaved += (_, _) => originalRaised = true;

        var sampleLogger = new TestLogger();
        var screen = new ServiceScreen<TestServiceOptions>(sampleLogger);
        bool sampleRaised = false;
        screen.ServiceSaved += (_, _) => sampleRaised = true;
        var sample = new ComposedTestService(new ServiceRule(), screen);

        var options = new TestServiceOptions(70000);

        var originalResult = original.Save("Test", options);
        var sampleResult = sample.Save("Test", options);

        Assert.False(originalResult);
        Assert.False(sampleResult);
        Assert.Equal(original.LastError, sample.LastError);
        Assert.False(originalRaised);
        Assert.False(sampleRaised);
        originalLogger.Entries.Should().BeEmpty();
        sampleLogger.Entries.Should().BeEmpty();
    }
}

