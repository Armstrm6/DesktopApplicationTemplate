using System;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.Helpers
{
    public class DependencyChecker
    {
        public static void CheckAll(ILoggingService? logger = null)
        {
            logger?.Log("[DependencyChecker] Running startup checks...", LogLevel.Debug);

            CheckDotNetRuntime(logger);
            CheckRequiredFiles(logger);
        }

        private static void CheckDotNetRuntime(ILoggingService? logger)
        {
            var version = Environment.Version;
            logger?.Log($"[DependencyChecker] .NET Version: {version}", LogLevel.Debug);

            if (version.Major < 8)
            {
                logger?.Log("[DependencyChecker] .NET 8 or higher is required.", LogLevel.Warning);
            }
        }

        private static void CheckRequiredFiles(ILoggingService? logger)
        {
            logger?.Log("[DependencyChecker] No binary resources to verify.", LogLevel.Debug);
        }
    }
}
