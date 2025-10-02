using System;
using System.Windows.Input;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    /// <summary>
    /// A generic relay command supporting a command parameter.
    /// </summary>
    /// <typeparam name="T">Parameter type.</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
        /// </summary>
        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <inheritdoc />
        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;

        /// <inheritdoc />
        public void Execute(object? parameter) => _execute((T?)parameter);

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
