using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.UI;
using Microsoft.VisualStudio.Threading;

namespace DesktopApplicationTemplate.UI.Helpers
{
    /// <summary>
    /// An <see cref="ICommand"/> implementation that executes asynchronous delegates.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private readonly SynchronizationContext? _synchronizationContext;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            _synchronizationContext = SynchronizationContext.Current;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _ = ExecuteAsync(parameter);

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="_">Unused parameter kept for signature compatibility.</param>
        public async Task ExecuteAsync(object? _ = null)
        {
            await _execute().ConfigureAwait(false);
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CommandDispatcher.FireAndForget(
                _synchronizationContext,
                CanExecuteChanged,
                this);
        }
    }

    /// <summary>
    /// An <see cref="ICommand"/> implementation that executes asynchronous delegates with a parameter.
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to the command.</typeparam>
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T?, Task> _execute;
        private readonly Predicate<T?>? _canExecute;
        // Capture the synchronization context so UI notifications mirror the non-generic command.
        private readonly SynchronizationContext? _synchronizationContext;
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        public AsyncRelayCommand(Func<T?, Task> execute, Predicate<T?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            _synchronizationContext = SynchronizationContext.Current;
        }

        /// <inheritdoc />
        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;

        /// <inheritdoc />
        public void Execute(object? parameter) => _ = ExecuteAsync((T?)parameter);

        /// <summary>
        /// Executes the command asynchronously with the provided parameter.
        /// </summary>
        public async Task ExecuteAsync(T? parameter)
        {
            await _execute(parameter).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Notifies that the ability to execute has changed.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandDispatcher.FireAndForget(
                _synchronizationContext,
                CanExecuteChanged,
                this);
        }
    }

    internal static class CommandDispatcher
    {
        public static async Task RaiseCanExecuteChanged(SynchronizationContext? synchronizationContext, EventHandler? handler, object sender)
        {
            if (handler is null)
            {
                return;
            }

            if (synchronizationContext is null || synchronizationContext == SynchronizationContext.Current)
            {
                handler(sender, EventArgs.Empty);
                return;
            }

            var joinableTaskFactory = App.UiThreadTaskFactory;
            if (joinableTaskFactory is null)
            {
                handler(sender, EventArgs.Empty);
                return;
            }

            await joinableTaskFactory.SwitchToMainThreadAsync();
            handler(sender, EventArgs.Empty);
        }

        public static Task RaiseCanExecuteChanged(EventHandler? handler, object sender)
        {
            return RaiseCanExecuteChanged(SynchronizationContext.Current, handler, sender);
        }

        public static void FireAndForget(SynchronizationContext? synchronizationContext, EventHandler? handler, object sender)
        {
            var joinableTaskFactory = App.UiThreadTaskFactory;
            var task = joinableTaskFactory is not null
                ? joinableTaskFactory.RunAsync(() => RaiseCanExecuteChanged(synchronizationContext, handler, sender)).Task
                : RaiseCanExecuteChanged(synchronizationContext, handler, sender);

            ObserveFailure(task);
        }

        private static void ObserveFailure(Task task)
        {
            if (task.IsCompleted)
            {
                if (task.IsFaulted)
                {
                    _ = task.Exception;
                }

                return;
            }

            task.ContinueWith(static t =>
            {
                _ = t.Exception;
            }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}
