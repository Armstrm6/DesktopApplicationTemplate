using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Services;

/// <summary>
/// Handles edit requests for a specific service type.
/// </summary>
public interface IEditServiceHandler
{
    /// <summary>
    /// Executes the edit workflow for the specified service.
    /// </summary>
    /// <param name="service">Service to edit.</param>
    void Edit(ServiceListModel service);
}
