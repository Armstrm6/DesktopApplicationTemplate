using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Threading;
using DesktopApplicationTemplate.Models;
using WpfRichTextBox = System.Windows.Controls.RichTextBox;

namespace DesktopApplicationTemplate.UI.Services
{
    public class RichTextLogger : IRichTextLogger
    {
        private readonly WpfRichTextBox _outputRichTextBox;
        private readonly Dispatcher _dispatcher;

        public RichTextLogger(WpfRichTextBox outputRichTextBox, Dispatcher dispatcher)
        {
            _outputRichTextBox = outputRichTextBox;
            _dispatcher = dispatcher;
        }

        public Task AppendAsync(LogEntry entry, CancellationToken cancellationToken = default)
        {
            return _dispatcher.InvokeAsync(() =>
            {
                var paragraph = new Paragraph(new Run(entry.Message) { Foreground = entry.Color });
                _outputRichTextBox.Document.Blocks.Add(paragraph);
                _outputRichTextBox.ScrollToEnd();
            }, DispatcherPriority.Background, cancellationToken).Task;
        }

        public Task SetEntriesAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default)
        {
            return _dispatcher.InvokeAsync(() =>
            {
                _outputRichTextBox.Document.Blocks.Clear();
                foreach (var e in entries)
                {
                    var paragraph = new Paragraph(new Run(e.Message) { Foreground = e.Color });
                    _outputRichTextBox.Document.Blocks.Add(paragraph);
                }
                _outputRichTextBox.ScrollToEnd();
            }, DispatcherPriority.Background, cancellationToken).Task;
        }
    }
}
