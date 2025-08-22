using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Helpers;
using System.Threading.Tasks;
using System.Net.Http;
using Moq;
using Moq.Protected;
using Xunit;
using System.Threading;

namespace DesktopApplicationTemplate.Tests
{
    public class TcpHttpViewModelLoggingTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void TcpService_ToggleServer_LogsMessage()
        {
            var logger = new Mock<ILoggingService>();
            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new TcpServiceViewModel(helper, new TcpServiceMessagesViewModel()) { Logger = logger.Object };
            vm.ComputerIp = "127.0.0.1";
            vm.ListeningPort = "5000";

            vm.ToggleServerCommand.Execute(null);

            logger.Verify(l => l.Log("Toggling server state", LogLevel.Debug), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task HttpService_InvalidUrl_LogsWarning()
        {
            var logger = new Mock<ILoggingService>();
            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new HttpServiceViewModel(helper) { Logger = logger.Object };
            vm.Url = string.Empty;

            await vm.SendRequestAsync();

            logger.Verify(l => l.Log(It.IsAny<string>(), LogLevel.Warning), Times.AtLeastOnce);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task HttpService_SendRequest_LogsLifecycle()
        {
            var logger = new Mock<ILoggingService>();
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("ok")
                });

            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new HttpServiceViewModel(helper) { Logger = logger.Object, MessageHandler = handler.Object, Url = "http://localhost/" };

            await vm.SendRequestAsync();

            logger.Verify(l => l.Log("Starting HTTP request", LogLevel.Debug), Times.Once);
            logger.Verify(l => l.Log("HTTP request completed", LogLevel.Debug), Times.Once);
            logger.Verify(l => l.Log("SendRequestAsync finished", LogLevel.Debug), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void HttpService_SetInvalidUrl_AddsError()
        {
            var logger = new Mock<ILoggingService>();
            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new HttpServiceViewModel(helper) { Logger = logger.Object };
            vm.Url = "htp://bad";

            Assert.True(vm.HasErrors);
            logger.Verify(l => l.Log(It.Is<string>(m => m.Contains("Invalid HTTP URL")), LogLevel.Warning), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
