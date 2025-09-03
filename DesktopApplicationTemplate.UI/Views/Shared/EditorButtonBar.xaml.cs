using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DesktopApplicationTemplate.UI.Views;

public partial class EditorButtonBar : UserControl
{
    public EditorButtonBar()
    {
        InitializeComponent();
    }

    public ICommand? AdvancedConfigCommand
    {
        get => (ICommand?)GetValue(AdvancedConfigCommandProperty);
        set => SetValue(AdvancedConfigCommandProperty, value);
    }

    public static readonly DependencyProperty AdvancedConfigCommandProperty =
        DependencyProperty.Register(nameof(AdvancedConfigCommand), typeof(ICommand), typeof(EditorButtonBar));

    public ICommand? SaveCommand
    {
        get => (ICommand?)GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    public static readonly DependencyProperty SaveCommandProperty =
        DependencyProperty.Register(nameof(SaveCommand), typeof(ICommand), typeof(EditorButtonBar));

    public ICommand? CancelCommand
    {
        get => (ICommand?)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    public static readonly DependencyProperty CancelCommandProperty =
        DependencyProperty.Register(nameof(CancelCommand), typeof(ICommand), typeof(EditorButtonBar));

    public string SaveButtonText
    {
        get => (string)GetValue(SaveButtonTextProperty);
        set => SetValue(SaveButtonTextProperty, value);
    }

    public static readonly DependencyProperty SaveButtonTextProperty =
        DependencyProperty.Register(nameof(SaveButtonText), typeof(string), typeof(EditorButtonBar), new PropertyMetadata("Save"));

    public string AdvancedAutomationName
    {
        get => (string)GetValue(AdvancedAutomationNameProperty);
        set => SetValue(AdvancedAutomationNameProperty, value);
    }

    public static readonly DependencyProperty AdvancedAutomationNameProperty =
        DependencyProperty.Register(nameof(AdvancedAutomationName), typeof(string), typeof(EditorButtonBar), new PropertyMetadata("Open Advanced Configuration"));

    public string SaveAutomationName
    {
        get => (string)GetValue(SaveAutomationNameProperty);
        set => SetValue(SaveAutomationNameProperty, value);
    }

    public static readonly DependencyProperty SaveAutomationNameProperty =
        DependencyProperty.Register(nameof(SaveAutomationName), typeof(string), typeof(EditorButtonBar), new PropertyMetadata("Save Service"));

    public string CancelAutomationName
    {
        get => (string)GetValue(CancelAutomationNameProperty);
        set => SetValue(CancelAutomationNameProperty, value);
    }

    public static readonly DependencyProperty CancelAutomationNameProperty =
        DependencyProperty.Register(nameof(CancelAutomationName), typeof(string), typeof(EditorButtonBar), new PropertyMetadata("Cancel"));
}
