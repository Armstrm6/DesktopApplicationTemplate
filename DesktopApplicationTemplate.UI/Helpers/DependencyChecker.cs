using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Helpers
{
    public class DependencyChecker
    {
        public static void CheckAll()
        {
            Console.WriteLine("[DependencyChecker] Running startup checks...");

            CheckDotNetRuntime();
            CheckRequiredFiles();
        }

        private static void CheckDotNetRuntime()
        {
            var version = Environment.Version;
            Console.WriteLine($"[DependencyChecker] .NET Version: {version}");

            // You could check minimum version requirement here
            if (version.Major < 8)
            {
                Console.WriteLine("[DependencyChecker] WARNING: .NET 8 or higher is required.");
            }
        }

        private static void CheckRequiredFiles()
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
                    Console.WriteLine($"[DependencyChecker] MISSING FILE: {file}");
                }
                else
                {
                    Console.WriteLine($"[DependencyChecker] OK: {file}");
                }
            }
        }
    }
}
