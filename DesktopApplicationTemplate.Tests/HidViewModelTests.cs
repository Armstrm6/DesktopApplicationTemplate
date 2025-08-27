using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Helpers;
using Moq;

namespace DesktopApplicationTemplate.Tests
{
    public class HidViewModelTests
    {
        [Fact]
        public void BuildMessage_FormatsAndForwards()
        {
            bool forwarded = false;
            MessageForwarder.ForwardAction = (svc, msg) =>
            {
                if (svc == "Target" && msg == ">test<")
                    forwarded = true;
            };

            var logger = new Mock<ILoggingService>();
            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new HidViewModel(helper) { Logger = logger.Object };
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
        public void DataFlowProperties_RaisePropertyChanged()
        {
            var logger = new Mock<ILoggingService>();
            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new HidViewModel(helper);

            string? lastProperty = null;
            vm.PropertyChanged += (_, e) => lastProperty = e.PropertyName;

            vm.IncomingData = "in";
            Assert.Equal("IncomingData", lastProperty);

            vm.ProcessingData = "proc";
            Assert.Equal("ProcessingData", lastProperty);

            vm.OutgoingData = "out";
            Assert.Equal("OutgoingData", lastProperty);

            ConsoleTestLogger.LogPass();
        }
    }
}
