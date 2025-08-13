using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Helpers;
using Moq;

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
            var vm = new HidViewModel(new SaveConfirmationHelper(logger.Object)) { Logger = logger.Object };
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
    }
}
