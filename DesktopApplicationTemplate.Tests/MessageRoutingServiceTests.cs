using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class MessageRoutingServiceTests
    {
        [Fact]
        public void UpdateMessage_PersistsAndReturnsLatest()
        {
            var routing = new MessageRoutingService();

            routing.UpdateMessage(ServiceType.Tcp, "svc", "first");
            routing.UpdateMessage(ServiceType.Tcp, "svc", "second");

            var found = routing.TryGetMessage(ServiceType.Tcp, "svc", out var message);

            Assert.True(found);
            Assert.Equal("second", message);
        }

        [Fact]
        public void ResolveTokens_ReplacesWithLatestMessage()
        {
            var routing = new MessageRoutingService();
            routing.UpdateMessage(ServiceType.Tcp, "svc", "hello");

            var result = routing.ResolveTokens("{Tcp.svc.Message}");

            Assert.Equal("hello", result);
        }

        [Fact]
        public void ResolveTokens_ReturnsEmpty_WhenUnknown()
        {
            var routing = new MessageRoutingService();
            var result = routing.ResolveTokens("{Tcp.missing.Message}");
            Assert.Equal(string.Empty, result);
        }
    }
}
