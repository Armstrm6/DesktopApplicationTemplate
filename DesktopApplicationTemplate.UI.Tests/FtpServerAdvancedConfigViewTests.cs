using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using FluentAssertions;
using Moq;

namespace DesktopApplicationTemplate.Tests;

[Collection("WpfTests")]
public class FtpServerAdvancedConfigViewTests
{
    [WpfFact]
    public void Initialize_SetsDataContext_AndLogger()
    {
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
        var view = new FtpServerAdvancedConfigView(new Mock<ILoggingService>().Object);

        Action act = () => view.Initialize(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("vm");
    }
}
