using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class FtpServerAdvancedConfigViewModelTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void RequiresCredentials_WhenNotAnonymous()
    {
        var options = new FtpServerOptions();
        var vm = new FtpServerAdvancedConfigViewModel(options);
        vm.AllowAnonymous = false;
        vm.Username = "";
        vm.Password = "";

        Assert.True(vm.HasErrors);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void SaveCommand_RaisesSaved()
    {
        var logger = new Mock<ILoggingService>();
        var options = new FtpServerOptions();
        var vm = new FtpServerAdvancedConfigViewModel(options, logger.Object)
        {
            AllowAnonymous = false,
            Username = "user",
            Password = "pass"
        };
        FtpServerOptions? saved = null;
        vm.Saved += o => saved = o;

        vm.SaveCommand.Execute(null);

        Assert.NotNull(saved);
        Assert.False(saved!.AllowAnonymous);
        Assert.Equal("user", saved.Username);
        Assert.Equal("pass", saved.Password);
        ConsoleTestLogger.LogPass();
    }
}
