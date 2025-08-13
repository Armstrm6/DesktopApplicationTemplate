using DesktopApplication.Installer.ViewModels;
using Xunit;
using System.IO;
using System.Linq;

namespace DesktopApplicationTemplate.Tests;

public class DepsJsonParsingTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void ParseRuntimeDependencies_ParsesRuntimeAndTargets()
    {
        var json = """
        {
          "targets": {
            "net8.0": {
              "Test.Lib/1.0.0": {
                "runtime": {
                  "lib/net8.0/Test.Lib.dll": {}
                },
                "runtimeTargets": {
                  "runtimes/win/native/libtest.dll": { "assetType": "native" }
                }
              }
            }
          }
        }
        """;

        var deps = ProgressWindowViewModel.ParseRuntimeDependencies(json).ToList();
        Assert.Contains(Path.Combine("lib", "net8.0", "Test.Lib.dll"), deps);
        Assert.Contains(Path.Combine("runtimes", "win", "native", "libtest.dll"), deps);
        ConsoleTestLogger.LogPass();
    }
}

