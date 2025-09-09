using System.Collections.Generic;
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

    public ScriptEditorWindow()
    {
        InitializeComponent();
        var vm = new ScriptEditorViewModel();
        DataContext = vm;
        vm.RequestClose += OnRequestClose;
        vm.ErrorsChanged += HighlightErrors;
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
        Editor.TextArea.TextView.LineTransformers.Clear();

        foreach (var diag in diagnostics)
        {
            var span = diag.Location.GetLineSpan();
            var lineNumber = span.StartLinePosition.Line + 1;
            Editor.TextArea.TextView.LineTransformers.Add(new LineHighlightTransformer(lineNumber, Colors.MistyRose));
        }
    }

    private sealed class LineHighlightTransformer : DocumentColorizingTransformer
    {
        private readonly int _lineNumber;
        private readonly SolidColorBrush _brush;

        public LineHighlightTransformer(int lineNumber, Color color)
        {
            _lineNumber = lineNumber;
            _brush = new SolidColorBrush(color);
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (line.LineNumber == _lineNumber)
            {
                ChangeLinePart(line.Offset, line.EndOffset,
                    element => element.TextRunProperties.SetBackgroundBrush(_brush));
            }
        }
    }
}
