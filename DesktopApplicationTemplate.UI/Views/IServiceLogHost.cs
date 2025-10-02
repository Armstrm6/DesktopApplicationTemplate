using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    /// <summary>
    /// Provides a hook for views that host a <see cref="ServiceLogView"/> to receive the associated service context.
    /// </summary>
    public interface IServiceLogHost
    {
        /// <summary>
        /// Sets the service context for the hosted log view.
        /// </summary>
        /// <param name="service">The service model containing log data.</param>
        void SetServiceContext(ServiceListModel service);
    }
}
