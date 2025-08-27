using System;
using System.IO;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Service.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class FileSearchServiceTests
{
    [Fact]
    public async Task GetFilesAsync_ReturnsMatchingFiles()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, "test.txt");
            await File.WriteAllTextAsync(file, "content");
            var svc = new FileSearchService(new NullLogger<FileSearchService>());

            var result = await svc.GetFilesAsync(dir, "*.txt");

            result.Should().ContainSingle().And.Contain(file);
            ConsoleTestLogger.LogPass();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public async Task GetFilesAsync_UsesCache()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        try
        {
            var first = Path.Combine(dir, "first.txt");
            await File.WriteAllTextAsync(first, "a");
            var svc = new FileSearchService(new NullLogger<FileSearchService>());

            var initial = await svc.GetFilesAsync(dir, "*.txt");
            initial.Should().HaveCount(1);

            var second = Path.Combine(dir, "second.txt");
            await File.WriteAllTextAsync(second, "b");

            var cached = await svc.GetFilesAsync(dir, "*.txt");
            cached.Should().HaveCount(1);
            ConsoleTestLogger.LogPass();
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}
