using DesktopApplicationTemplate.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class WorkerTests
    {
        [Fact]
        public void Constructor_LoadsHeartbeatSettings()
        {
            var settings = new Dictionary<string, string?>
            {
                {"Heartbeat:Message", "TEST"},
                {"Heartbeat:IntervalSeconds", "10"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            var logger = NullLogger<Worker>.Instance;
            var factory = NullLoggerFactory.Instance;
            var worker = new Worker(logger, factory, configuration);

            Assert.Equal("TEST", worker.HeartbeatMessage);
            Assert.Equal(10, worker.IntervalSeconds);

            ConsoleTestLogger.LogPass();
        }
    }
}
