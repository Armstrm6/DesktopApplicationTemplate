using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DesktopApplicationTemplate.UI.Views;

public partial class AdvancedConfigButtonBar : UserControl
{
    public AdvancedConfigButtonBar()
    {
        InitializeComponent();
    }

    public ICommand? SaveCommand
    {
        get => (ICommand?)GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    public static readonly DependencyProperty SaveCommandProperty =
        DependencyProperty.Register(nameof(SaveCommand), typeof(ICommand), typeof(AdvancedConfigButtonBar));

    public ICommand? BackCommand
    {
        get => (ICommand?)GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public static readonly DependencyProperty BackCommandProperty =
        DependencyProperty.Register(nameof(BackCommand), typeof(ICommand), typeof(AdvancedConfigButtonBar));

    public string SaveButtonText
    {
        get => (string)GetValue(SaveButtonTextProperty);
        set => SetValue(SaveButtonTextProperty, value);
    }

    public static readonly DependencyProperty SaveButtonTextProperty =
        DependencyProperty.Register(nameof(SaveButtonText), typeof(string), typeof(AdvancedConfigButtonBar), new PropertyMetadata("Save"));

    public string SaveAutomationName
    {
        get => (string)GetValue(SaveAutomationNameProperty);
        set => SetValue(SaveAutomationNameProperty, value);
    }

    public static readonly DependencyProperty SaveAutomationNameProperty =
        DependencyProperty.Register(nameof(SaveAutomationName), typeof(string), typeof(AdvancedConfigButtonBar), new PropertyMetadata("Save Advanced Options"));

    public string BackAutomationName
    {
        get => (string)GetValue(BackAutomationNameProperty);
        set => SetValue(BackAutomationNameProperty, value);
    }

    public static readonly DependencyProperty BackAutomationNameProperty =
        DependencyProperty.Register(nameof(BackAutomationName), typeof(string), typeof(AdvancedConfigButtonBar), new PropertyMetadata("Back"));
}
