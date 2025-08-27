using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Core.Services
{
    public interface IProcessRunner
    {
        Task RunAsync(string fileName, string arguments, CancellationToken cancellationToken = default);
    }
}
