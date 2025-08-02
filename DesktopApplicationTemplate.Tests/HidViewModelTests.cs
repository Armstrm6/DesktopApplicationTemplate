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
        public void BuildMessage_FormatsAndForwards()
        {
            bool forwarded = false;
            MessageForwarder.ForwardAction = (svc, msg) =>
            {
                if (svc == "Target" && msg == ">test<")
                    forwarded = true;
            };

            var logger = new Mock<ILoggingService>();
            var vm = new HidViewModel { Logger = logger.Object };
            vm.AvailableServices.Add("Target");
            vm.AttachedService = "Target";
            vm.MessageTemplate = "test";
            vm.FormatTemplate = ">\u007b0\u007d<";

            vm.BuildCommand.Execute(null);

            Assert.Equal(">test<", vm.FinalMessage);
            Assert.True(forwarded);
            logger.Verify(l => l.Log("Building HID message", LogLevel.Debug), Times.Once);
            MessageForwarder.ForwardAction = null;

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void BuildCommand_InvokesHidService()
        {
            var hidService = new Mock<IHidService>();
            hidService.Setup(s => s.SendAsync(">msg<", 0, 0, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var vm = new HidViewModel(hidService.Object);
            vm.MessageTemplate = "msg";
            vm.FormatTemplate = ">\u007b0\u007d<";

            vm.BuildCommand.Execute(null);

            hidService.Verify(s => s.SendAsync(">msg<", 0, 0, It.IsAny<CancellationToken>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
