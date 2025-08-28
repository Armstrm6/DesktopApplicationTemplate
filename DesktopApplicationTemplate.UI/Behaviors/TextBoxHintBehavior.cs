using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DesktopApplicationTemplate.UI.Behaviors;

/// <summary>
/// Provides automatic tooltip text for <see cref="TextBox"/> controls based on their bound property name.
/// </summary>
public static class TextBoxHintBehavior
{
    /// <summary>
    /// Identifies the AutoToolTip attached property. When set to <c>true</c>,
    /// the behavior assigns a tooltip generated from the bound property name.
    /// </summary>
    public static readonly DependencyProperty AutoToolTipProperty =
        DependencyProperty.RegisterAttached(
            "AutoToolTip",
            typeof(bool),
            typeof(TextBoxHintBehavior),
            new PropertyMetadata(false, OnAutoToolTipChanged));

    /// <summary>
    /// Gets the value of the <see cref="AutoToolTipProperty"/> for the specified object.
    /// </summary>
    /// <param name="obj">The dependency object.</param>
    /// <returns>The current value.</returns>
    public static bool GetAutoToolTip(DependencyObject obj)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        return (bool)obj.GetValue(AutoToolTipProperty);
    }

    /// <summary>
    /// Sets the value of the <see cref="AutoToolTipProperty"/> for the specified object.
    /// </summary>
    /// <param name="obj">The dependency object.</param>
    /// <param name="value">The value to set.</param>
    public static void SetAutoToolTip(DependencyObject obj, bool value)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        obj.SetValue(AutoToolTipProperty, value);
    }

    private static void OnAutoToolTipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if (e.NewValue is true)
            {
                textBox.Loaded += TextBoxLoaded;
            }
            else
            {
                textBox.Loaded -= TextBoxLoaded;
            }
        }
    }

    private static void TextBoxLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        var binding = BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty);
        var propertyPath = binding?.ParentBinding?.Path?.Path;
        if (string.IsNullOrWhiteSpace(propertyPath))
        {
            return;
        }

        if (textBox.ToolTip is null)
        {
            textBox.ToolTip = GetFriendlyName(propertyPath);
        }
    }

    /// <summary>
    /// Converts a property path such as <c>ServiceName</c> into a spaced string like "Service Name".
    /// </summary>
    /// <param name="propertyPath">The binding path.</param>
    /// <returns>A friendly name.</returns>
    internal static string GetFriendlyName(string propertyPath)
    {
        if (propertyPath is null)
        {
            throw new ArgumentNullException(nameof(propertyPath));
        }

        var lastSegmentIndex = propertyPath.LastIndexOf('.');
        var propertyName = lastSegmentIndex >= 0 ? propertyPath[(lastSegmentIndex + 1)..] : propertyPath;

        return Regex.Replace(propertyName, "(\\B[A-Z])", " $1");
    }
}
