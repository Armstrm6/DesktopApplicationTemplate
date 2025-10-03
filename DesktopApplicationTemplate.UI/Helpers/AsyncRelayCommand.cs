using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _ = ExecuteAsync(parameter);

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">Unused parameter for signature compatibility.</param>
        public async Task ExecuteAsync(object? parameter = null)
        {
            await _execute().ConfigureAwait(false);
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged() => CommandDispatcher.RaiseCanExecuteChanged(CanExecuteChanged, this);
    }

    /// <summary>
    /// An <see cref="ICommand"/> implementation that executes asynchronous delegates with a parameter.
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to the command.</typeparam>
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T?, Task> _execute;
        private readonly Predicate<T?>? _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        public AsyncRelayCommand(Func<T?, Task> execute, Predicate<T?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
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
        public void RaiseCanExecuteChanged() => CommandDispatcher.RaiseCanExecuteChanged(CanExecuteChanged, this);
    }

    internal static class CommandDispatcher
    {
        private static readonly Lazy<JoinableTaskFactory?> UiThreadFactory = new Lazy<JoinableTaskFactory?>(CreateUiThreadFactory);

        public static void RaiseCanExecuteChanged(EventHandler? handler, object sender)
        {
            if (handler is null)
            {
                return;
            }

            var factory = UiThreadFactory.Value;

            if (factory is null)
            {
                handler(sender, EventArgs.Empty);
                return;
            }

            if (factory.Context.IsOnMainThread)
            {
                handler(sender, EventArgs.Empty);
                return;
            }

            factory.Run(async () =>
            {
                await factory.SwitchToMainThreadAsync();
                handler(sender, EventArgs.Empty);
            });
        }

        private static JoinableTaskFactory? CreateUiThreadFactory()
        {
            var dispatcher = Application.Current?.Dispatcher;

            if (dispatcher is null)
            {
                return null;
            }

            return dispatcher.Invoke(() =>
            {
                var synchronizationContext = SynchronizationContext.Current ?? new DispatcherSynchronizationContext(dispatcher);
                var context = new JoinableTaskContext(dispatcher.Thread, synchronizationContext);
                return context.Factory;
            });
        }
    }
}
