using System;
using DesktopApplicationTemplate.UI.Services;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class MessageRoutingServiceTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        public void ResolveTokens_ReplacesWithLatestMessage()
        {
            var routing = new MessageRoutingService();
            routing.UpdateMessage("svc", "hello");

            var result = routing.ResolveTokens("{svc.Message}");

            Assert.Equal("hello", result);
        }

        [Fact]
        [TestCategory("CodexSafe")]
        public void ResolveTokens_ReturnsEmpty_WhenUnknown()
        {
            var routing = new MessageRoutingService();
            var result = routing.ResolveTokens("{missing.Message}");
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        [TestCategory("CodexSafe")]
        public void UpdateMessage_Throws_WhenServiceNameNullOrWhitespace()
        {
            var routing = new MessageRoutingService();

            Assert.Throws<ArgumentException>(() => routing.UpdateMessage(" ", "msg"));
        }

        [Fact]
        [TestCategory("CodexSafe")]
        public void TryGetMessage_ReturnsLatestMessage()
        {
            var routing = new MessageRoutingService();
            routing.UpdateMessage("svc", "first");
            routing.UpdateMessage("svc", "second");

            var found = routing.TryGetMessage("svc", out var message);

            Assert.True(found);
            Assert.Equal("second", message);
        }
    }
}
