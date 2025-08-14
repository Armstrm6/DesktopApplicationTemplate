using DesktopApplicationTemplate.UI.Models;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class MqttServiceOptionsTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void Defaults_AreCorrect()
        {
            var options = new MqttServiceOptions();
            Assert.Equal("localhost", options.Host);
            Assert.Contains(1883, options.Ports);
            Assert.Equal("mqtt", options.Protocol);
            Assert.Equal(string.Empty, options.Username);
            Assert.Equal(string.Empty, options.Password);
            Assert.Equal("client1", options.ClientId);
            Assert.Equal(60, options.KeepAlive);
            Assert.True(options.CleanSession);
            Assert.Equal(0, options.QoS);
            Assert.False(options.RetainFlag);
            Assert.Equal(5, options.ReconnectDelay);
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void Ports_Throws_When_OutOfRange()
        {
            var options = new MqttServiceOptions();
            Assert.Throws<ArgumentOutOfRangeException>(() => options.Ports = new[] { 0 });
            Assert.Throws<ArgumentOutOfRangeException>(() => options.Ports = new[] { 70000 });
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void KeepAlive_Throws_When_OutOfRange()
        {
            var options = new MqttServiceOptions();
            Assert.Throws<ArgumentOutOfRangeException>(() => options.KeepAlive = -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => options.KeepAlive = 70000);
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void QoS_Throws_When_OutOfRange()
        {
            var options = new MqttServiceOptions();
            Assert.Throws<ArgumentOutOfRangeException>(() => options.QoS = -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => options.QoS = 3);
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void ReconnectDelay_Throws_When_OutOfRange()
        {
            var options = new MqttServiceOptions();
            Assert.Throws<ArgumentOutOfRangeException>(() => options.ReconnectDelay = -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => options.ReconnectDelay = 4000);
        }
    }
}
