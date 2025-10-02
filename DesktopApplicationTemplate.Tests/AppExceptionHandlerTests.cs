using System;
using System.Windows.Threading;
using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.UI.Services;
using FluentAssertions;
using Xunit;
using System.Threading.Tasks;
using System.Reflection;

namespace DesktopApplicationTemplate.Tests
{
    public class AppExceptionHandlerTests
    {
        [Fact]
        public void DispatcherUnhandledException_InvokesHookRelease()
        {
            var app = new App();
            var called = false;
            KeyboardSimulator.ResetAction = () => called = true;
            var args = (DispatcherUnhandledExceptionEventArgs)Activator.CreateInstance(
                typeof(DispatcherUnhandledExceptionEventArgs),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[] { Dispatcher.CurrentDispatcher, new InvalidOperationException() },
                null)!;

            app.OnDispatcherUnhandledException(app, args);

            called.Should().BeTrue();
            args.Handled.Should().BeTrue();
            KeyboardSimulator.ResetAction = null;
        }

        [Fact]
        public async Task AppDomainUnhandledException_InvokesHookRelease()
        {
            var app = new App();
            var called = false;
            KeyboardSimulator.ResetAction = () => called = true;
            var args = new UnhandledExceptionEventArgs(new InvalidOperationException(), false);

            await app.OnAppDomainUnhandledExceptionAsync(app, args);

            called.Should().BeTrue();
            KeyboardSimulator.ResetAction = null;
        }
    }
}
