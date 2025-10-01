using System.Collections.Generic;
using System.Linq;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Modules;
using DesktopApplicationTemplate.UI.ViewModels;
using FluentAssertions;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class ServicePresentationBindingTests
{
    [Fact]
    public void ServiceListModel_UsesDescriptorMetadataForBuiltInServices()
    {
        var module = new UiServiceModule();
        var descriptors = module.DescribeServices()
            .Where(descriptor => descriptor.LegacyType.HasValue)
            .ToList();

        var expected = new Dictionary<ServiceType, ExpectedSnapshot>
        {
            [ServiceType.Csv] = new("CSV Creator", "📄", "#FFD3D3D3", "#FF808080", "Generate CSV output from message payloads."),
            [ServiceType.FileObserver] = new("File Observer", string.Empty, "#FFFFA07A", "#FFE9967A", "Monitor directories for changes and trigger automation flows."),
            [ServiceType.Ftp] = new("FTP Server", "🖥️", "#FFB0C4DE", "#FF4682B4", "Transfer files to remote hosts using the FTP protocol."),
            [ServiceType.Heartbeat] = new("Heartbeat", string.Empty, "#FFFFB6C1", "#FFFF1493", "Emit periodic heartbeat messages for monitoring integrations."),
            [ServiceType.Hid] = new("HID", string.Empty, "#FFFFFFE0", "#FFDAA520", "Interact with Human Interface Devices for automation scenarios."),
            [ServiceType.Http] = new("HTTP", "🌐", "#FF90EE90", "#FF006400", "Interact with HTTP endpoints for automation workflows."),
            [ServiceType.Mqtt] = new("MQTT", "📡", "#FFFAFAD2", "#FFDAA520", "Connect to MQTT brokers and manage topic subscriptions."),
            [ServiceType.Scp] = new("SCP", "📦", "#FFE0FFFF", "#FF5F9EA0", "Transfer files securely using the SCP protocol."),
            [ServiceType.Tcp] = new("TCP", "🔗", "#FFADD8E6", "#FF00008B", "Send and receive messages over TCP or UDP."),
        };

        foreach (var descriptor in descriptors)
        {
            var legacyType = descriptor.LegacyType!.Value;
            var service = new ServiceListModel { Type = legacyType };

            service.ApplyDescriptor(descriptor);

            var snapshot = expected[legacyType];
            service.DisplayName.Should().Be(snapshot.Label);
            service.DescriptorLabel.Should().Be(snapshot.Label);
            (service.IconGlyph ?? string.Empty).Should().Be(snapshot.IconGlyph);
            service.DescriptorDescription.Should().Be(snapshot.Description);
            service.DescriptorTooltip.Should().Be(snapshot.ExpectedTooltip);
            service.BackgroundColor.ToString().Should().Be(snapshot.BackgroundColor);
            service.BorderColor.ToString().Should().Be(snapshot.BorderColor);
        }
    }

    private sealed record ExpectedSnapshot(
        string Label,
        string IconGlyph,
        string BackgroundColor,
        string BorderColor,
        string Description)
    {
        public string ExpectedTooltip => string.IsNullOrWhiteSpace(Description)
            ? Label
            : $"{Label}: {Description}";
    }
}
