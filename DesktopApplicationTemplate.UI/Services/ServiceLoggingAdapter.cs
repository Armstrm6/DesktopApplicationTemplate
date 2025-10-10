using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Decorates an <see cref="ILoggingService"/> so log entries are scoped to a single service instance.
    /// </summary>
    public sealed class ServiceLoggingAdapter : ILoggingService, IDisposable
    {
        private readonly ILoggingService inner;
        private readonly ServiceType serviceType;
        private readonly Func<string> serviceNameProvider;
        private bool disposed;

        public ServiceLoggingAdapter(
            ILoggingService inner,
            ServiceType serviceType,
            Func<string> serviceNameProvider)
        {
            this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
            this.serviceType = serviceType;
            this.serviceNameProvider = serviceNameProvider ?? throw new ArgumentNullException(nameof(serviceNameProvider));

            this.inner.LogAdded += HandleInnerLogAdded;
        }

        /// <summary>
        /// Gets the decorated logger.
        /// </summary>
        public ILoggingService InnerLogger => inner;

        public LogLevel MinimumLevel
        {
            get => inner.MinimumLevel;
            set => inner.MinimumLevel = value;
        }

        public event Action<LogEntry>? LogAdded;

        public void Log(string message, LogLevel level)
        {
            var serviceName = GetServiceName();
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                inner.Log(message, level);
                return;
            }

            inner.Log($"{serviceType}.{serviceName}.{message}", level);
        }

        public Task ReloadAsync(CancellationToken cancellationToken = default)
        {
            return inner.ReloadAsync(cancellationToken);
        }

        private void HandleInnerLogAdded(LogEntry entry)
        {
            if (entry is null)
            {
                return;
            }

            var serviceName = GetServiceName();
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return;
            }

            if (entry.ServiceType == serviceType &&
                string.Equals(entry.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase))
            {
                LogAdded?.Invoke(entry);
            }
        }

        private string GetServiceName()
        {
            var name = serviceNameProvider()?.Trim();
            return string.IsNullOrWhiteSpace(name) ? string.Empty : name;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            inner.LogAdded -= HandleInnerLogAdded;
            disposed = true;
        }
    }
}
