using System;
using System.Linq;
using System.Threading;
using System.Windows;
using DesktopApplicationTemplate.UI;

namespace DesktopApplicationTemplate.Tests;

public static class ApplicationResourceHelper
{
    private static readonly Uri FormsUri = new("/DesktopApplicationTemplate.UI;component/Themes/Forms.xaml", UriKind.Relative);

    public static void EnsureApplication()
    {
        if (Application.Current is null)
        {
            _ = new App();
        }

        var app = Application.Current!;
        if (!app.Resources.MergedDictionaries.Any(d => d.Source == FormsUri))
        {
            app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = FormsUri });
        }
    }

    public static void RunOnDispatcher(Action action)
    {
        Exception? ex = null;
        var thread = new Thread(() =>
        {
            try
            {
                EnsureApplication();
                action();
            }
            catch (Exception e) { ex = e; }
            finally
            {
                Application.Current?.Shutdown();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (ex != null) throw ex;
    }
}
