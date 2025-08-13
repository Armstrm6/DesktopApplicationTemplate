using DesktopApplicationTemplate.UI.Helpers;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class DependencyCheckerTests
    {
        [Fact]
        public void CheckAll_LogsNoBinaryResourcesMessage()
        {
            var testLogger = new TestLogger();

            DependencyChecker.CheckAll(testLogger);

            Assert.Contains(testLogger.Entries, entry => entry.Message.Contains("No binary resources to verify"));
        }
    }
}
