using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Documents;
using DesktopApplicationTemplate.UI.Helpers;
using FluentAssertions;

namespace DesktopApplicationTemplate.Tests;

public class VisualTreeHelperExtensionsTests
{
    [WindowsFact]
    public void FindParent_Returns_TextBlock_For_Inline()
    {
        Exception? exception = null;

        var thread = new Thread(() =>
        {
            try
            {
                var run = new Run("child");
                var textBlock = new TextBlock(run);

                var parent = VisualTreeHelperExtensions.FindParent<TextBlock>(run);

                parent.Should().BeSameAs(textBlock);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        exception.Should().BeNull();
    }
}

