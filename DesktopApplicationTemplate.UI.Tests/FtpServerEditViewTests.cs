using System;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using FluentAssertions;

namespace DesktopApplicationTemplate.Tests;

[Collection("WpfTests")]
public class FtpServerEditViewTests
{
    [WindowsFact]
    public void Constructor_SetsDataContext()
    {
        var vm = new FtpServerEditViewModel("ftp", new());
        var view = new FtpServerEditView(vm);

        view.DataContext.Should().BeOfType<FtpServerEditViewModel>();
    }

    [WindowsFact]
    public void Constructor_Throws_When_ViewModelNull()
    {
        Action act = () => new FtpServerEditView(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("viewModel");
    }
}
