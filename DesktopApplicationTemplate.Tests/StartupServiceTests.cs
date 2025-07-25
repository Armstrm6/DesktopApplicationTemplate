using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.Collections.Generic;

namespace DesktopApplicationTemplate.Tests
{
    public class StartupServiceTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        public void GetSettings_ReturnsExpectedValues()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"AppSettings:Environment", "Test"},
                {"AppSettings:ServerIP", "192.168.1.1"},
                {"AppSettings:ServerPort", "1234"},
                {"AppSettings:LogLevel", "Information"},
                {"AppSettings:AutoStart", "false"},
                {"AppSettings:DefaultCSharpScriptPath", "/path/script.cs"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var startupService = new StartupService(configuration);

            AppSettings settings = startupService.GetSettings();

            Assert.Equal("Test", settings.Environment);
            Assert.Equal("192.168.1.1", settings.ServerIP);
            Assert.Equal(1234, settings.ServerPort);
            Assert.Equal("Information", settings.LogLevel);
            Assert.False(settings.AutoStart);
            Assert.Equal("/path/script.cs", settings.DefaultCSharpScriptPath);

            ConsoleTestLogger.LogPass();
        }
    }
}
