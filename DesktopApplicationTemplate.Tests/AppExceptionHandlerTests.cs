using System;
using System.Windows.Threading;
using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.UI.Helpers;
using FluentAssertions;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class AppExceptionHandlerTests
    {
        [Fact]
        public void DispatcherUnhandledException_InvokesHookRelease()
        {
            var app = new App();
            var called = false;
            HookReleaseHelper.ReleaseAction = () => called = true;
            var args = new DispatcherUnhandledExceptionEventArgs(Dispatcher.CurrentDispatcher, new InvalidOperationException(), false);

            app.OnDispatcherUnhandledException(app, args);

            called.Should().BeTrue();
            args.Handled.Should().BeTrue();
        }

        [Fact]
        public void AppDomainUnhandledException_InvokesHookRelease()
        {
            var app = new App();
            var called = false;
            HookReleaseHelper.ReleaseAction = () => called = true;
            var args = new UnhandledExceptionEventArgs(new InvalidOperationException(), false);

            app.OnAppDomainUnhandledException(app, args);

            called.Should().BeTrue();
        }
    }
}
