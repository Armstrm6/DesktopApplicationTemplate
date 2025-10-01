using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class CreateServiceViewModel : ViewModelBase
    {
        public record ServiceDescriptorMetadata(
            string DescriptorId,
            string DisplayLabel,
            string IconGlyph,
            ServiceType? LegacyType,
            string Category,
            string? Description,
            string PrimaryAccentColor,
            string SecondaryAccentColor);

        private readonly IServiceCatalog _catalog;
        private readonly Dictionary<string, ServiceType> _descriptorLegacyTypes;
        private readonly HashSet<string> _existingNames;

        public CreateServiceViewModel(IServiceCatalog catalog, IEnumerable<string>? existingNames = null)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _existingNames = existingNames != null
                ? new HashSet<string>(existingNames)
                : new HashSet<string>();

            _descriptorLegacyTypes = new Dictionary<string, ServiceType>(StringComparer.Ordinal);
            ServiceDescriptors = new ObservableCollection<ServiceDescriptorMetadata>();
            _catalog.DescriptorsChanged += (_, _) => RefreshServiceDescriptors();
            RefreshServiceDescriptors();
        }

        public ObservableCollection<ServiceDescriptorMetadata> ServiceDescriptors { get; }

        public bool TryGetLegacyType(string descriptorId, out ServiceType serviceType)
        {
            if (descriptorId is null)
            {
                serviceType = default;
                return false;
            }

            return _descriptorLegacyTypes.TryGetValue(descriptorId, out serviceType);
        }

        public string GenerateDefaultName(string descriptorId)
        {
            var baseName = ResolveBaseName(descriptorId);
            var index = 1;
            var candidate = FormattableString.Invariant($"{baseName}{index}");
            while (_existingNames.Contains(candidate))
            {
                index++;
                candidate = FormattableString.Invariant($"{baseName}{index}");
            }

            return candidate;
        }

        private string ResolveBaseName(string descriptorId)
        {
            if (!string.IsNullOrWhiteSpace(descriptorId) && _catalog.TryGetById(descriptorId, out var descriptor))
            {
                if (descriptor.LegacyType is ServiceType legacyType)
                {
                    return legacyType.ToLegacyString();
                }

                var label = descriptor.Presentation.DisplayLabel;
                if (!string.IsNullOrWhiteSpace(label))
                {
                    return label!;
                }

                if (!string.IsNullOrWhiteSpace(descriptor.DisplayName))
                {
                    return descriptor.DisplayName;
                }
            }

            return descriptorId;
        }

        // OnPropertyChanged provided by ViewModelBase

        private void RefreshServiceDescriptors()
        {
            var descriptorsWithLegacy = _catalog.Descriptors
                .Select(descriptor =>
                {
                    var presentation = descriptor.Presentation;
                    var displayLabel = !string.IsNullOrWhiteSpace(presentation.DisplayLabel)
                        ? presentation.DisplayLabel!
                        : descriptor.DisplayName;
                    var iconGlyph = !string.IsNullOrWhiteSpace(presentation.IconGlyph)
                        ? presentation.IconGlyph!
                        : string.Empty;
                    var primary = !string.IsNullOrWhiteSpace(presentation.PrimaryAccentColor)
                        ? presentation.PrimaryAccentColor!
                        : string.Empty;
                    var secondary = !string.IsNullOrWhiteSpace(presentation.SecondaryAccentColor)
                        ? presentation.SecondaryAccentColor!
                        : string.Empty;

                    return new ServiceDescriptorMetadata(
                        descriptor.Id,
                        displayLabel,
                        iconGlyph,
                        descriptor.LegacyType,
                        descriptor.Category,
                        descriptor.Description,
                        primary,
                        secondary);
                })
                .ToList();

            _descriptorLegacyTypes.Clear();
            foreach (var metadata in descriptorsWithLegacy)
            {
                if (metadata.LegacyType is { } legacyType)
                {
                    _descriptorLegacyTypes[metadata.DescriptorId] = legacyType;
                }
            }

            ServiceDescriptors.Clear();
            foreach (var metadata in descriptorsWithLegacy)
            {
                ServiceDescriptors.Add(metadata);
            }
        }
    }
}
