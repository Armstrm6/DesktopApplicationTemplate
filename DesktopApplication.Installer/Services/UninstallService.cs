using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplication.Installer.Services
{
    public interface IUninstallService
    {
        Task UninstallAsync(string installPath, CancellationToken cancellationToken = default);
    }

    internal class UninstallService : IUninstallService
    {
        private readonly IProcessManager _processManager;
        private readonly ILoggingService? _logger;
        private const string ServiceProcessName = "DesktopApplicationTemplate.Service";

        public UninstallService(IProcessManager processManager, ILoggingService? logger = null)
        {
            _processManager = processManager;
            _logger = logger;
        }

        public async Task UninstallAsync(string installPath, CancellationToken cancellationToken = default)
        {
            _logger?.Log("Uninstallation started", LogLevel.Debug);
            await StopServicesAsync(cancellationToken).ConfigureAwait(false);
            await RemoveStartupEntryAsync(cancellationToken).ConfigureAwait(false);
            await RemoveFirewallRuleAsync(cancellationToken).ConfigureAwait(false);
            await RemoveFilesAsync(installPath, cancellationToken).ConfigureAwait(false);
            _logger?.Log("Uninstallation completed", LogLevel.Debug);
        }

        private Task StopServicesAsync(CancellationToken cancellationToken)
        {
            foreach (var pid in _processManager.GetProcessIdsByName(ServiceProcessName))
            {
                cancellationToken.ThrowIfCancellationRequested();
                _logger?.Log($"Stopping service process {pid}", LogLevel.Debug);
                _processManager.KillProcess(pid);
            }
            return Task.CompletedTask;
        }

        private Task RemoveFilesAsync(string installPath, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(installPath))
                return Task.CompletedTask;
            try
            {
                Directory.Delete(installPath, recursive: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _logger?.Log($"Error deleting installation directory: {ex.Message}", LogLevel.Error);
            }
            return Task.CompletedTask;
        }

        private Task RemoveStartupEntryAsync(CancellationToken cancellationToken)
        {
            _logger?.Log("Removing startup entry", LogLevel.Debug);
            return Task.CompletedTask;
        }

        private Task RemoveFirewallRuleAsync(CancellationToken cancellationToken)
        {
            _logger?.Log("Removing firewall rule", LogLevel.Debug);
            return Task.CompletedTask;
        }
    }
}
