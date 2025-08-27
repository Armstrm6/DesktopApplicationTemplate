using System.Threading;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using FluentAssertions;

namespace DesktopApplicationTemplate.Tests;

public class FtpServerEditViewTests
{
    [Fact]
    public void Constructor_SetsDataContext()
    {
        FtpServerEditView? view = null;
        var thread = new Thread(() =>
        {
            var vm = new FtpServerEditViewModel("ftp", new());
            view = new FtpServerEditView(vm);
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        view!.DataContext.Should().BeOfType<FtpServerEditViewModel>();
    }

    [Fact]
    public void Constructor_Throws_When_ViewModelNull()
    {
        Exception? ex = null;
        var thread = new Thread(() =>
        {
            try
            {
                _ = new FtpServerEditView(null!);
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
            .Which.ParamName.Should().Be("viewModel");
    }
}
