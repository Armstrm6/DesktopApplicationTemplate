using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class FtpServerViewViewModelTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void FileReceived_AddsToUploadedFiles()
    {
        var service = new Mock<IFtpServerService>();
        var vm = new FtpServerViewViewModel(service.Object);

        var args = new FtpTransferEventArgs("/upload.txt", 10, true);
        service.Raise(s => s.FileReceived += null!, args);

        Assert.Single(vm.UploadedFiles);
        Assert.Equal("/upload.txt", vm.UploadedFiles[0].Path);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void FileSent_AddsToDownloadedFiles()
    {
        var service = new Mock<IFtpServerService>();
        var vm = new FtpServerViewViewModel(service.Object);

        var args = new FtpTransferEventArgs("/download.txt", 20, false);
        service.Raise(s => s.FileSent += null!, args);

        Assert.Single(vm.DownloadedFiles);
        Assert.Equal("/download.txt", vm.DownloadedFiles[0].Path);
        ConsoleTestLogger.LogPass();
    }
}
