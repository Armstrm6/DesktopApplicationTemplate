using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public sealed class PluginLoaderIntegrationTests : IDisposable
{
    private readonly string rootPath;

    public PluginLoaderIntegrationTests()
    {
        rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootPath);
    }

    [Fact]
    public void LoaderRegistersDescriptorsFromPluginArchive()
    {
        var pluginSource = Path.Combine(rootPath, "package");
        Directory.CreateDirectory(pluginSource);

        var pluginsDirectory = Path.Combine(rootPath, "plugins");
        Directory.CreateDirectory(pluginsDirectory);

        var extractionDirectory = Path.Combine(rootPath, "cache");

        var assemblyPath = Path.Combine(pluginSource, "SamplePlugin.dll");
        CompilePluginAssembly(assemblyPath);

        var manifestPath = Path.Combine(pluginSource, "plugin.manifest.json");
        File.WriteAllText(manifestPath, CreateManifestJson());

        var archivePath = Path.Combine(pluginsDirectory, "SamplePlugin.zip");
        ZipFile.CreateFromDirectory(pluginSource, archivePath);

        var options = new PluginLoaderOptions(pluginsDirectory, extractionDirectory);
        var loader = new PluginLoader(options, NullLogger<PluginLoader>.Instance);
        var pluginAssemblies = loader.LoadPluginAssemblies();

        pluginAssemblies.Should().NotBeEmpty();

        var services = new ServiceCollection();
        var assembliesForScanning = PluginLoader.CombineWithDefaultAssemblies(pluginAssemblies);
        var catalog = services.AddServiceModules(assembliesForScanning.ToArray());

        catalog.Descriptors.Should().Contain(descriptor => descriptor.Id == "Sample.Plugin.Service");
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
        var pluginSourceCode = string.Join(
            Environment.NewLine,
            "using System;",
            "using System.Collections.Generic;",
            "using DesktopApplicationTemplate.Core.Modules;",
            "using DesktopApplicationTemplate.Core.Services;",
            "using DesktopApplicationTemplate.Models;",
            "using Microsoft.Extensions.DependencyInjection;",
            string.Empty,
            "namespace SamplePlugin;",
            string.Empty,
            "public sealed class SamplePluginModule : IServiceModule",
            "{",
            "    public void RegisterServices(IServiceCollection services)",
            "    {",
            "    }",
            string.Empty,
            "    public IEnumerable<IServiceDescriptor> DescribeServices()",
            "    {",
            "        yield return new SampleDescriptor();",
            "    }",
            string.Empty,
            "    private sealed class SampleDescriptor : IServiceDescriptor",
            "    {",
            "        public string Id => \"Sample.Plugin.Service\";",
            "        public string DisplayName => \"Sample Plugin Service\";",
            "        public string Category => \"Plugins\";",
            "        public string? Description => \"Sample descriptor\";",
            "        public ServiceType? LegacyType => null;",
            "        public IServiceOptionsSerializer? OptionsSerializer => null;",
            "        public IReadOnlyCollection<ServiceFactoryBinding> Factories => Array.Empty<ServiceFactoryBinding>();",
            "        public ServicePresentationMetadata Presentation => ServicePresentationMetadata.Empty;",
            "        public bool HasPayloadDescription => false;",
            "        public string? DescribePayload(object? payload) => null;",
            "    }",
            "}");

        var syntaxTree = CSharpSyntaxTree.ParseText(pluginSourceCode);

        var references = GetCompilationReferences();
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

    private static IEnumerable<MetadataReference> GetCompilationReferences()
    {
        var assemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location));

        return assemblies.Select(assembly => MetadataReference.CreateFromFile(assembly.Location));
    }

    private static string CreateManifestJson() => string.Join(
        Environment.NewLine,
        "{",
        "  \"id\": \"Sample.Plugin\",",
        "  \"name\": \"Sample Plugin\",",
        "  \"version\": \"1.0.0\",",
        "  \"entryAssembly\": \"SamplePlugin.dll\",",
        "  \"serviceAssemblies\": [ \"SamplePlugin.dll\" ]",
        "}");
}
