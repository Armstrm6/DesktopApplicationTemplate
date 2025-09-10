using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using DesktopApplicationTemplate.UI.Helpers;

namespace DesktopApplicationTemplate.UI.ViewModels;

public class ScriptEditorViewModel : ViewModelBase
{
    public const string DefaultScript = "string Process(string message)\n{\n    return message;\n}";

    private string _scriptText = DefaultScript;
    private string _testMessage = string.Empty;
    private string _outputMessage = string.Empty;
    private Brush _outputBrush = Brushes.Black;
    private string _lastTestMessage = string.Empty;

    public string ScriptText
    {
        get => _scriptText;
        set
        {
            if (_scriptText != value)
            {
                _scriptText = value;
                OnPropertyChanged();
            }
        }
    }

    public string TestMessage
    {
        get => _testMessage;
        set
        {
            if (_testMessage != value)
            {
                _testMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public string OutputMessage
    {
        get => _outputMessage;
        set
        {
            if (_outputMessage != value)
            {
                _outputMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public Brush OutputBrush
    {
        get => _outputBrush;
        set
        {
            if (_outputBrush != value)
            {
                _outputBrush = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand RunCommand { get; }
    public ICommand SaveCommand { get; }

    public event EventHandler<ScriptSavedEventArgs>? RequestClose;
    public event Action<IEnumerable<Diagnostic>>? ErrorsChanged;

    public ScriptEditorViewModel()
    {
        RunCommand = new AsyncRelayCommand(RunAsync);
        SaveCommand = new RelayCommand(Save);
    }

    private async Task RunAsync()
    {
        var globals = new Globals { message = TestMessage };
        var code = ScriptText + "\nProcess(message);";
        var script = CSharpScript.Create<string>(code, ScriptOptions.Default, typeof(Globals));
        var diagnostics = script.Compile();
        await Application.Current.Dispatcher.InvokeAsync(() => ErrorsChanged?.Invoke(diagnostics));

        if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                OutputMessage = string.Join(Environment.NewLine, diagnostics.Select(d => d.ToString()));
                OutputBrush = Brushes.Red;
            });
            return;
        }

        try
        {
            var result = await script.RunAsync(globals);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                OutputMessage = result.ReturnValue;
                OutputBrush = Brushes.Black;
                _lastTestMessage = TestMessage;
            });
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                OutputMessage = ex.ToString();
                OutputBrush = Brushes.Red;
            });
        }
    }

    private void Save() => RequestClose?.Invoke(this, new ScriptSavedEventArgs(ScriptText, _lastTestMessage));

    public class Globals
    {
        public string message = string.Empty;
    }
}

public class ScriptSavedEventArgs : EventArgs
{
    public ScriptSavedEventArgs(string script, string testMessage)
    {
        Script = script;
        TestMessage = testMessage;
    }

    public string Script { get; }
    public string TestMessage { get; }
}
