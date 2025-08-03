using DesktopApplicationTemplate.UI.ViewModels;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Tests
{
    public class AsyncRelayCommandTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task Execute_AwaitsTask()
        {
            bool executed = false;
            var cmd = new AsyncRelayCommand(async () =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                executed = true;
            });

            cmd.Execute(null);
            await Task.Delay(20).ConfigureAwait(false);

            Assert.True(executed);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void CanExecute_PreventsReentry()
        {
            var tcs = new TaskCompletionSource();
            var cmd = new AsyncRelayCommand(async () => await tcs.Task);

            Assert.True(cmd.CanExecute(null));
            cmd.Execute(null);
            Assert.False(cmd.CanExecute(null));
            tcs.SetResult();

            ConsoleTestLogger.LogPass();
        }
    }
}
