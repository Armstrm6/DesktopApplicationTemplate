using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class FtpServiceViewModelTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void FileReceived_AddsToUploadedFiles()
    {
        var service = new Mock<IFtpServerService>();
        var vm = new FtpServiceViewModel(service.Object);

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
        var vm = new FtpServiceViewModel(service.Object);

        var args = new FtpTransferEventArgs("/download.txt", 20, false);
        service.Raise(s => s.FileSent += null!, args);

        Assert.Single(vm.DownloadedFiles);
        Assert.Equal("/download.txt", vm.DownloadedFiles[0].Path);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void TransferProgress_UpdatesTransfers()
    {
        var service = new Mock<IFtpServerService>();
        var vm = new FtpServiceViewModel(service.Object);

        var progress = new FtpTransferProgressEventArgs("/file.txt", 100, 50, true);
        service.Raise(s => s.TransferProgress += null!, progress);

        Assert.Single(vm.Transfers);
        Assert.Equal(0.5, vm.Transfers[0].Progress);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void ClientCountChanged_UpdatesProperty()
    {
        var service = new Mock<IFtpServerService>();
        var vm = new FtpServiceViewModel(service.Object);

        // EventHandler<int> has both sender and count parameters, so provide both when raising
        service.Raise(s => s.ClientCountChanged += null!, service.Object, 3);

        Assert.Equal(3, vm.ConnectedClients);
        ConsoleTestLogger.LogPass();
    }
}
