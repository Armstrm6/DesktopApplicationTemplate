using DesktopApplicationTemplate.UI;
using System;
using System.Windows;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class BubblyWindowStyleTests
    {
        [WindowsFact]
        public void BubblyWindowStyle_LoadsWithRoundedCorners()
        {
            ApplicationResourceHelper.RunOnDispatcher(() =>
            {
                var resourceDictionary = new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/DesktopApplicationTemplate.UI;component/Themes/BubblyWindow.xaml")
                };

                Assert.True(resourceDictionary.Contains("BubblyWindowStyle"));
                _ = (Style)resourceDictionary["BubblyWindowStyle"];

                ConsoleTestLogger.LogPass();
            });
        }
    }
}
