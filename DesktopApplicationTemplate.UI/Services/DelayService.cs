using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Standard implementation of <see cref="IDelayService"/>.
    /// </summary>
    public class DelayService : IDelayService
    {
        public Task DelayAsync(int milliseconds, CancellationToken token)
        {
            return Task.Delay(milliseconds, token);
        }
    }
}
