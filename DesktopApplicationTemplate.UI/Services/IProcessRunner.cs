using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    public interface IProcessRunner
    {
        Task RunAsync(string fileName, string arguments, CancellationToken cancellationToken = default);
    }
}
