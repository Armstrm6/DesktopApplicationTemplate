using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
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
        foreach (DocumentLine line in Editor.Document.Lines)
        {
            line.BackgroundColor = null;
        }

        foreach (var diag in diagnostics)
        {
            var span = diag.Location.GetLineSpan();
            var line = Editor.Document.GetLineByNumber(span.StartLinePosition.Line + 1);
            line.BackgroundColor = Colors.MistyRose;
        }
    }
}
