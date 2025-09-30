using System;

namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Provides UI presentation guidance for a service descriptor that can be consumed without referencing UI assemblies.
/// </summary>
public sealed record ServicePresentationMetadata(
    string? IconGlyph,
    string? PrimaryAccentColor,
    string? SecondaryAccentColor,
    string? DisplayLabel)
{
    /// <summary>
    /// Gets an empty metadata instance used when no presentation hints are provided.
    /// </summary>
    public static ServicePresentationMetadata Empty { get; } = new(null, null, null, null);

    /// <summary>
    /// Gets a value indicating whether the metadata contains any presentation hints.
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(IconGlyph)
        && string.IsNullOrWhiteSpace(PrimaryAccentColor)
        && string.IsNullOrWhiteSpace(SecondaryAccentColor)
        && string.IsNullOrWhiteSpace(DisplayLabel);

    /// <summary>
    /// Normalizes the provided metadata by returning <see cref="Empty"/> when <paramref name="metadata"/> is <c>null</c>.
    /// </summary>
    /// <param name="metadata">The metadata instance to normalize.</param>
    /// <returns>The normalized metadata.</returns>
    public static ServicePresentationMetadata Normalize(ServicePresentationMetadata? metadata) => metadata ?? Empty;
}
