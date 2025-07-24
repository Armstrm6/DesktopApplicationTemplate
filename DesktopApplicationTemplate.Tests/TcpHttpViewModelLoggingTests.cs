using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Core.Services;
using System.Threading.Tasks;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class TcpHttpViewModelLoggingTests
    {
        [Fact]
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
