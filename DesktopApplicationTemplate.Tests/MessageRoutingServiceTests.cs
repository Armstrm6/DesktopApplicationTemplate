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
    }
}
