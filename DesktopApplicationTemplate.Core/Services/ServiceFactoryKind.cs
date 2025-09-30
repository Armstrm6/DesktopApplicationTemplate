namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Identifies the integration point for a service factory binding.
/// </summary>
public enum ServiceFactoryKind
{
    /// <summary>
    /// Factory used to integrate with user interface workflows.
    /// </summary>
    UserInterface,

    /// <summary>
    /// Factory used to provision runtime/background services.
    /// </summary>
    Runtime
}
