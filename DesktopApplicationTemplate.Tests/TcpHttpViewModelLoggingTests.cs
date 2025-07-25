using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;
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
        public void TcpService_ToggleServer_LogsMessage()
        {
            var logger = new TestLogger();
            var vm = new TcpServiceViewModel { Logger = logger };
            vm.ComputerIp = "127.0.0.1";
            vm.ListeningPort = "5000";

            vm.ToggleServerCommand.Execute(null);

            Assert.Equal(2, logger.Entries.Count);
            Assert.Equal("Toggling server state", logger.Entries[0].Message);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        public async Task HttpService_InvalidUrl_LogsWarning()
        {
            var logger = new TestLogger();
            var vm = new HttpServiceViewModel { Logger = logger };
            vm.Url = string.Empty;

            await vm.SendRequestAsync();

            Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Warning, logger.Entries[0].Level);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        public async Task HttpService_SendRequest_LogsLifecycle()
        {
            var logger = new TestLogger();
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("ok")
                });

            var vm = new HttpServiceViewModel { Logger = logger, MessageHandler = handler.Object, Url = "http://localhost/" };

            await vm.SendRequestAsync();

            Assert.Contains(logger.Entries, e => e.Message == "Starting HTTP request");
            Assert.Contains(logger.Entries, e => e.Message == "HTTP request completed");
            Assert.Contains(logger.Entries, e => e.Message == "SendRequestAsync finished");

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        public void HttpService_SetInvalidUrl_AddsError()
        {
            var logger = new TestLogger();
            var vm = new HttpServiceViewModel { Logger = logger };
            vm.Url = "htp://bad";

            Assert.True(vm.HasErrors);
            Assert.Contains(logger.Entries, e => e.Message.Contains("Invalid HTTP URL"));

            ConsoleTestLogger.LogPass();
        }
    }
}
