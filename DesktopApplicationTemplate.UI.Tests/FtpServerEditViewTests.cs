using System;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using FluentAssertions;

namespace DesktopApplicationTemplate.Tests;

[Collection("Application")]
public class FtpServerEditViewTests
{
    [WpfFact]
    public void Constructor_SetsDataContext()
    {
        var vm = new FtpServerEditViewModel("ftp", new());
        var view = new FtpServerEditView(vm);

        view.DataContext.Should().BeOfType<FtpServerEditViewModel>();
    }

    [WpfFact]
    public void Constructor_Throws_When_ViewModelNull()
    {
        Action act = () => new FtpServerEditView(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("viewModel");
    }
}
