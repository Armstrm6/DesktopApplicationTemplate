using System;
using System.Linq;
using System.Windows;

namespace DesktopApplicationTemplate.Tests;

public static class ApplicationResourceHelper
{
    private static readonly Uri FormsUri = new("/DesktopApplicationTemplate.UI;component/Themes/Forms.xaml", UriKind.Relative);

    public static void EnsureApplication()
    {
        var app = Application.Current;

        if (app is null)
        {
            return;
        }

        if (!app.Resources.MergedDictionaries.Any(d => d.Source == FormsUri))
        {
            app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = FormsUri });
        }
    }
}
