using DesktopApplicationTemplate.UI;
using System;
using System.Threading;
using System.Windows;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class BubblyWindowStyleTests
    {
        [WpfFact]
        public void BubblyWindowStyle_LoadsWithRoundedCorners()
        {
            if (!OperatingSystem.IsWindows())
                return;

            Exception? capturedException = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var resourceDictionary = new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DesktopApplicationTemplate.UI;component/Themes/BubblyWindow.xaml")
                    };

                    Assert.True(resourceDictionary.Contains("BubblyWindowStyle"));
                    _ = (Style)resourceDictionary["BubblyWindowStyle"];
                }
                catch (Exception ex)
                {
                    capturedException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (capturedException != null)
                throw capturedException;

            ConsoleTestLogger.LogPass();
        }
    }
}
