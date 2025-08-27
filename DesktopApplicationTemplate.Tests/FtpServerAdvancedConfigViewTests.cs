using System;
using System.Threading;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using FluentAssertions;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class FtpServerAdvancedConfigViewTests
{
    [WpfFact]
    public void Initialize_SetsDataContext_AndLogger()
    {
        FtpServerAdvancedConfigView? view = null;
        FtpServerAdvancedConfigViewModel? vm = null;
        ILoggingService? logger = null;
        var thread = new Thread(() =>
        {
            logger = new Mock<ILoggingService>().Object;
            vm = new FtpServerAdvancedConfigViewModel(new FtpServerOptions());
            view = new FtpServerAdvancedConfigView(logger);
            view.Initialize(vm);
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        view!.DataContext.Should().Be(vm);
        vm!.Logger.Should().BeSameAs(logger);
    }

    [WpfFact]
    public void Initialize_Throws_When_ViewModelNull()
    {
        Exception? ex = null;
        var thread = new Thread(() =>
        {
            var view = new FtpServerAdvancedConfigView(new Mock<ILoggingService>().Object);
            try
            {
                view.Initialize(null!);
            }
            catch (Exception e)
            {
                ex = e;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        ex.Should().BeOfType<ArgumentNullException>()
            .Which.ParamName.Should().Be("vm");
    }
}
