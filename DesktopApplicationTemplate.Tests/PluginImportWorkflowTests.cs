using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public sealed class PluginImportWorkflowTests : IDisposable
{
    private readonly string rootPath;

    public PluginImportWorkflowTests()
    {
        rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootPath);
    }

    [Fact]
    public async Task ImportingPluginAddsDescriptorToCreateServiceViewModel()
    {
        var pluginSource = Path.Combine(rootPath, "plugin");
        Directory.CreateDirectory(pluginSource);

        var pluginsDirectory = Path.Combine(rootPath, "plugins");
        Directory.CreateDirectory(pluginsDirectory);

        var extractionDirectory = Path.Combine(rootPath, "cache");

        var assemblyPath = Path.Combine(pluginSource, "SamplePlugin.dll");
        CompilePluginAssembly(assemblyPath);

        var manifestPath = Path.Combine(pluginSource, "plugin.manifest.json");
        File.WriteAllText(manifestPath, CreateManifestJson());

        var archivePath = Path.Combine(pluginsDirectory, "SamplePlugin.ccp");
        ZipFile.CreateFromDirectory(pluginSource, archivePath);

        var options = new PluginLoaderOptions(pluginsDirectory, extractionDirectory);
        var catalog = new FakeServiceCatalog();
        var loggerFactory = NullLoggerFactory.Instance;
        var importService = new PluginImportService(options, catalog, NullLogger<PluginImportService>.Instance, loggerFactory);
        var createViewModel = new CreateServiceViewModel(catalog);

        Assert.Empty(createViewModel.ServiceDescriptors);

        var result = await importService.ImportAsync(archivePath);
        Assert.True(result.Success);
        Assert.NotEmpty(result.ImportedDescriptors);

        var metadata = Assert.Single(createViewModel.ServiceDescriptors);
        Assert.Equal("Plugins", metadata.Category);
        Assert.Equal("Demo descriptor", metadata.Description);
        Assert.Equal("✨", metadata.IconGlyph);
        Assert.Equal("#FF112233", metadata.PrimaryAccentColor);
        Assert.Equal("#FF445566", metadata.SecondaryAccentColor);

        ConsoleTestLogger.LogPass();
    }

    public void Dispose()
    {
        if (Directory.Exists(rootPath))
        {
            Directory.Delete(rootPath, recursive: true);
        }
    }

    private static void CompilePluginAssembly(string outputPath)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.DependencyInjection;

namespace SamplePlugin;

public sealed class SamplePluginModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
    }

    public IEnumerable<IServiceDescriptor> DescribeServices()
    {
        yield return new SampleDescriptor();
    }

    private sealed class SampleDescriptor : IServiceDescriptor
    {
        public string Id => \"Sample.Plugin.Service\";
        public string DisplayName => \"Sample Plugin Service\";
        public string Category => \"Plugins\";
        public string? Description => \"Demo descriptor\";
        public ServiceType? LegacyType => ServiceType.Tcp;
        public IServiceOptionsSerializer? OptionsSerializer => null;
        public IReadOnlyCollection<ServiceFactoryBinding> Factories => System.Array.Empty<ServiceFactoryBinding>();
        public ServicePresentationMetadata Presentation => new(\"✨\", \"#FF112233\", \"#FF445566\", \"Plugin Service\");
        public bool HasPayloadDescription => false;
        public string? DescribePayload(object? payload) => null;
    }
}
");

        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));

        var compilation = CSharpCompilation.Create(
            Path.GetFileNameWithoutExtension(outputPath),
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var result = compilation.Emit(outputPath);
        if (!result.Success)
        {
            var diagnostics = string.Join(Environment.NewLine, result.Diagnostics.Select(d => d.ToString()));
            throw new InvalidOperationException($"Failed to build sample plug-in: {diagnostics}");
        }
    }

    private static string CreateManifestJson() => @"{
  \"id\": \"Sample.Plugin\",
  \"name\": \"Sample Plugin\",
  \"version\": \"1.0.0\",
  \"entryAssembly\": \"SamplePlugin.dll\",
  \"serviceAssemblies\": [ \"SamplePlugin.dll\" ]
}";
}
