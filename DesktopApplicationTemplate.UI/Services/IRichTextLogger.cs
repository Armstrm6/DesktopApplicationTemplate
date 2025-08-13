using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Services
{
    public interface IRichTextLogger
    {
        Task AppendAsync(LogEntry entry, CancellationToken cancellationToken = default);
        Task SetEntriesAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default);
    }
}
