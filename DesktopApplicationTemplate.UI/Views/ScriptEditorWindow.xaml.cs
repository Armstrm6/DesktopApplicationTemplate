using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.CodeAnalysis;

namespace DesktopApplicationTemplate.UI.Views;

public partial class ScriptEditorWindow : Window
{
    public string ScriptText { get; private set; } = string.Empty;
    public string LastTestMessage { get; private set; } = string.Empty;
    private readonly List<DocumentColorizingTransformer> _errorTransformers = new();

    public ScriptEditorWindow()
    {
        InitializeComponent();
        var vm = new ScriptEditorViewModel();
        DataContext = vm;
        Editor.Text = vm.ScriptText;
        Editor.TextChanged += (_, _) => vm.ScriptText = Editor.Text;
        vm.PropertyChanged += OnViewModelPropertyChanged;
        vm.RequestClose += OnRequestClose;
        vm.ErrorsChanged += HighlightErrors;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ScriptEditorViewModel vm || e.PropertyName != nameof(ScriptEditorViewModel.ScriptText))
            return;

        if (Editor.Text != vm.ScriptText)
            Editor.Text = vm.ScriptText;
    }

    private void OnRequestClose(object? sender, ScriptSavedEventArgs e)
    {
        ScriptText = e.Script;
        LastTestMessage = e.TestMessage;
        DialogResult = true;
        Close();
    }

    private void HighlightErrors(IEnumerable<Diagnostic> diagnostics)
    {
        ClearErrorHighlights();

        foreach (var diag in diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error && !diag.Location.IsInMetadata))
        {
            var span = diag.Location.GetLineSpan();
            try
            {
                var startOffset = Editor.Document.GetOffset(span.StartLinePosition.Line + 1, span.StartLinePosition.Character + 1);
                var endOffset = Editor.Document.GetOffset(span.EndLinePosition.Line + 1, span.EndLinePosition.Character + 1);
                var transformer = new ErrorUnderlineTransformer(startOffset, endOffset);
                _errorTransformers.Add(transformer);
                Editor.TextArea.TextView.LineTransformers.Add(transformer);
            }
            catch
            {
                // Ignore malformed spans.
            }
        }

        Editor.TextArea.TextView.Redraw();
    }

    private void ClearErrorHighlights()
    {
        foreach (var transformer in _errorTransformers)
        {
            Editor.TextArea.TextView.LineTransformers.Remove(transformer);
        }
        _errorTransformers.Clear();
    }

    private sealed class ErrorUnderlineTransformer : DocumentColorizingTransformer
    {
        private readonly int _startOffset;
        private readonly int _endOffset;

        public ErrorUnderlineTransformer(int startOffset, int endOffset)
        {
            _startOffset = Math.Max(startOffset, 0);
            _endOffset = Math.Max(endOffset, _startOffset);
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            var lineStart = Math.Max(line.Offset, _startOffset);
            var lineEnd = Math.Min(line.EndOffset, _endOffset);
            if (lineStart >= lineEnd)
            {
                return;
            }

            ChangeLinePart(lineStart, lineEnd, element =>
            {
                var decorations = element.TextRunProperties.TextDecorations;
                if (decorations is null || decorations.IsFrozen)
                {
                    decorations = decorations != null ? decorations.Clone() : new TextDecorationCollection();
                }

                var underline = new TextDecoration
                {
                    Location = TextDecorationLocation.Underline,
                    Pen = new Pen(Brushes.Red, 1),
                    PenThicknessUnit = TextDecorationUnit.FontRecommended
                };

                decorations.Add(underline);
                element.TextRunProperties.SetTextDecorations(decorations);
            });
        }
    }
}
