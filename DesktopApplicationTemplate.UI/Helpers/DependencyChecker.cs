using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopApplicationTemplate.UI.Services;

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
            var basePath = AppContext.BaseDirectory;
            var requiredFiles = new[] {
                    "Resources/mascot-body.png",
                    "Resources/mascot-head.png",
                    "Resources/mascot-eye1.png"
                    }.Select(f => Path.Combine(basePath, f)).ToArray();

            foreach (var file in requiredFiles)
            {
                if (!File.Exists(file))
                {
                    logger?.Log($"[DependencyChecker] MISSING FILE: {file}", LogLevel.Warning);
                }
                else
                {
                    logger?.Log($"[DependencyChecker] OK: {file}", LogLevel.Debug);
                }
            }
        }
    }
}
