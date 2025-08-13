using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    /// <summary>
    /// Provides a logger property for view models that support logging.
    /// </summary>
    public interface ILoggingViewModel
    {
        /// <summary>
        /// Gets or sets the associated logger.
        /// </summary>
        ILoggingService? Logger { get; set; }
    }
}
