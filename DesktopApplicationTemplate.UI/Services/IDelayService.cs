using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Provides a testable abstraction over Task.Delay.
    /// </summary>
    public interface IDelayService
    {
        Task DelayAsync(int milliseconds, CancellationToken token);
    }
}
