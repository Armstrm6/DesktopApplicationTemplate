using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Moq;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Tests
{
    public class HidViewModelTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task BuildMessage_FormatsForwardsAndSends()
        {
            bool forwarded = false;
            MessageForwarder.ForwardAction = (svc, msg) =>
            {
                if (svc == "Target" && msg == ">test<")
                    forwarded = true;
            };

            var logger = new Mock<ILoggingService>();
            var hidService = new Mock<IHidService>();
            var sendTcs = new TaskCompletionSource();
            hidService.Setup(s => s.SendAsync(">test<", 0, 0, It.IsAny<CancellationToken>()))
                .Returns(() => { sendTcs.SetResult(); return Task.CompletedTask; });

            var vm = new HidViewModel(hidService.Object, logger.Object);
            vm.AvailableServices.Add("Target");
            vm.AttachedService = "Target";
            vm.MessageTemplate = "test";
            vm.FormatTemplate = ">\u007b0\u007d<";

            vm.BuildCommand.Execute(null);
            await sendTcs.Task;

            Assert.Equal(">test<", vm.FinalMessage);
            Assert.True(forwarded);
            hidService.Verify(s => s.SendAsync(">test<", 0, 0, It.IsAny<CancellationToken>()), Times.Once);
            logger.Verify(l => l.Log("Building HID message", LogLevel.Debug), Times.Once);
            MessageForwarder.ForwardAction = null;

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void BuildMessage_LogsWhenLoggingEnabled()
        {
            var logger = new Mock<ILoggingService>();
            var hidService = new Mock<IHidService>();

            var vm = new HidViewModel(hidService.Object, logger.Object)
            {
                IsLoggingHidData = true,
                VbaLogContent = "log"
            };

            vm.BuildCommand.Execute(null);

            hidService.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            logger.Verify(l => l.Log("Logging HID data: log", LogLevel.Information), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
