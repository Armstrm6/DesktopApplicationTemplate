using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    /// <summary>
    /// Base view model that supports validation using <see cref="INotifyDataErrorInfo"/>.
    /// </summary>
    public abstract class ValidatableViewModelBase : ViewModelBase, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errors = new();

        /// <inheritdoc />
        public bool HasErrors => _errors.Any();

        /// <inheritdoc />
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        /// <summary>
        /// Retrieves validation errors for the specified property.
        /// </summary>
        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return _errors.SelectMany(e => e.Value);
            return _errors.TryGetValue(propertyName, out var list) ? list : Enumerable.Empty<string>();
        }

        /// <summary>
        /// Adds an error to the specified property.
        /// </summary>
        protected void AddError(string property, string error)
        {
            if (!_errors.ContainsKey(property))
                _errors[property] = new List<string>();
            if (!_errors[property].Contains(error))
            {
                _errors[property].Add(error);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(property));
            }
        }

        /// <summary>
        /// Clears any errors for the specified property.
        /// </summary>
        protected void ClearErrors(string property)
        {
            if (_errors.Remove(property))
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(property));
        }
    }
}
