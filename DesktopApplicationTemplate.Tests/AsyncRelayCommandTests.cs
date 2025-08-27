using DesktopApplicationTemplate.UI.Helpers;
using System.Threading.Tasks;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class AsyncRelayCommandTests
    {
        [Fact]
        public async Task Execute_InvokesAsyncDelegate()
        {
            var invoked = false;
            var command = new AsyncRelayCommand(async () =>
            {
                await Task.Delay(10);
                invoked = true;
            });

            await command.ExecuteAsync();

            Assert.True(invoked);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public void CanExecute_RespectsPredicate()
        {
            var command = new AsyncRelayCommand(() => Task.CompletedTask, () => false);

            Assert.False(command.CanExecute(null));
            ConsoleTestLogger.LogPass();
        }
    }
}
