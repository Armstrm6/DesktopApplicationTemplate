using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Services
{
    public class NullRichTextLogger : IRichTextLogger
    {
        public Task AppendAsync(LogEntry entry, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SetEntriesAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
