using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Helpers;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xunit;
using Moq;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.Tests
{
    public class LogLevelSelectionTests
    {
        [Fact]
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

                    var helper = new SaveConfirmationHelper(new Mock<ILoggingService>().Object);
                    var viewModel = new HttpServiceViewModel(helper);
                    var logger = new LoggingService(new NullRichTextLogger());
                    viewModel.Logger = logger;
                    var view = new HttpServiceView(viewModel, logger);

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

                    var concrete = Assert.IsType<LoggingService>(viewModel.Logger);
                    Assert.Equal(LogLevel.Error, concrete.MinimumLevel);
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
