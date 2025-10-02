using System.Windows;
using System.Windows.Controls;

namespace DesktopApplicationTemplate.UI.Views;

public partial class ServiceEditorView : Page
{
    public ServiceEditorView()
    {
        InitializeComponent();
    }

    public string AdvancedAutomationName
    {
        get => (string)GetValue(AdvancedAutomationNameProperty);
        set => SetValue(AdvancedAutomationNameProperty, value);
    }

    public static readonly DependencyProperty AdvancedAutomationNameProperty =
        DependencyProperty.Register(nameof(AdvancedAutomationName), typeof(string), typeof(ServiceEditorView), new PropertyMetadata("Open Advanced Configuration"));

    public string SaveAutomationName
    {
        get => (string)GetValue(SaveAutomationNameProperty);
        set => SetValue(SaveAutomationNameProperty, value);
    }

    public static readonly DependencyProperty SaveAutomationNameProperty =
        DependencyProperty.Register(nameof(SaveAutomationName), typeof(string), typeof(ServiceEditorView), new PropertyMetadata("Save Service"));

    public string CancelAutomationName
    {
        get => (string)GetValue(CancelAutomationNameProperty);
        set => SetValue(CancelAutomationNameProperty, value);
    }

    public static readonly DependencyProperty CancelAutomationNameProperty =
        DependencyProperty.Register(nameof(CancelAutomationName), typeof(string), typeof(ServiceEditorView), new PropertyMetadata("Cancel"));
}
