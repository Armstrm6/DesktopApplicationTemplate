using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class SaveConfirmationHelperDiTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    public void SaveConfirmationHelper_Throws_WhenLoggerMissing()
    {
        var services = new ServiceCollection();
        services.AddSingleton<SaveConfirmationHelper>();
        using var provider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<SaveConfirmationHelper>());
    }

    [Fact]
    [TestCategory("CodexSafe")]
    public void SaveConfirmationHelper_Resolves_WhenLoggerRegistered()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IRichTextLogger, NullRichTextLogger>();
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<SaveConfirmationHelper>();
        using var provider = services.BuildServiceProvider();

        var helper = provider.GetRequiredService<SaveConfirmationHelper>();
        Assert.NotNull(helper);
    }
}
