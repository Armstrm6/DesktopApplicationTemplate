using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using FluentAssertions;
using Moq;

namespace DesktopApplicationTemplate.Tests;

public class FtpServerAdvancedConfigViewTests
{
    [WpfFact]
    public void Initialize_SetsDataContext_AndLogger()
    {
        ApplicationResourceHelper.EnsureApplication();

        var logger = new Mock<ILoggingService>().Object;
        var vm = new FtpServerAdvancedConfigViewModel(new FtpServerOptions());
        var view = new FtpServerAdvancedConfigView(logger);

        view.Initialize(vm);

        view.DataContext.Should().Be(vm);
        vm.Logger.Should().BeSameAs(logger);
    }

    [WpfFact]
    public void Initialize_Throws_When_ViewModelNull()
    {
        ApplicationResourceHelper.EnsureApplication();

        var view = new FtpServerAdvancedConfigView(new Mock<ILoggingService>().Object);

        Action act = () => view.Initialize(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("vm");
    }
}
