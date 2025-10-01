using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DesktopApplicationTemplate.UI.Helpers;

public sealed class StringToBrushConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var colorValue = value as string;
        if (!string.IsNullOrWhiteSpace(colorValue))
        {
            try
            {
                return (Brush)new BrushConverter().ConvertFromString(colorValue)!;
            }
            catch (FormatException)
            {
                return Brushes.Transparent;
            }
        }

        if (parameter is string fallback && !string.IsNullOrWhiteSpace(fallback))
        {
            try
            {
                return (Brush)new BrushConverter().ConvertFromString(fallback)!;
            }
            catch (FormatException)
            {
                return Brushes.Transparent;
            }
        }

        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
