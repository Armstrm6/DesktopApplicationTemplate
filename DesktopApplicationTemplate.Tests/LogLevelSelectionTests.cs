using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Services;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class LogLevelSelectionTests
    {
        [Fact]
        [TestCategory("WindowsSafe")]
        public void HttpServiceView_LogLevelSelection_UpdatesLogger()
        {
            if (!OperatingSystem.IsWindows())
                return;

            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try
                {
                    if (System.Windows.Application.Current == null)
                        new DesktopApplicationTemplate.UI.App();

                    var viewModel = new HttpServiceViewModel();
                    var view = new HttpServiceView(viewModel);

                    var targetItem = view.LogLevelBox.Items
                        .OfType<ComboBoxItem>()
                        .First(i => (string?)i.Content == "Error");
                    view.LogLevelBox.SelectedItem = targetItem;

                    var method = typeof(HttpServiceView).GetMethod(
                        "LogLevelBox_SelectionChanged",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    method?.Invoke(view, new object[]
                    {
                        view.LogLevelBox,
                        new SelectionChangedEventArgs(
                            Selector.SelectionChangedEvent,
                            Array.Empty<object>(),
                            Array.Empty<object>())
                    });

                    var logger = Assert.IsType<LoggingService>(viewModel.Logger);
                    Assert.Equal(LogLevel.Error, logger.MinimumLevel);
                }
                catch (Exception e)
                {
                    ex = e;
                }
                finally
                {
                    System.Windows.Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;

            ConsoleTestLogger.LogPass();
        }
    }
}
