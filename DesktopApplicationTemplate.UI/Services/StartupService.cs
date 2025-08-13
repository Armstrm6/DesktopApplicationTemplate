using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.Services
{
    public class StartupService : IStartupService
    {
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;
        private readonly ILoggingService? _logger;

        public StartupService(IConfiguration configuration, ILoggingService? logger = null)
        {
            _configuration = configuration;
            _appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
            _logger = logger;
        }


        public async Task RunStartupChecksAsync()
        {
            _logger?.Log($"Environment: {_appSettings.Environment}", LogLevel.Debug);
            await Task.Run(() => DependencyChecker.CheckAll(_logger));

            if (_appSettings.AutoStart)
            {
                EnableAutoStart();
                _logger?.Log("Auto-start enabled", LogLevel.Debug);
            }
            else
            {
                DisableAutoStart();
                _logger?.Log("Auto-start disabled", LogLevel.Debug);
            }
        }

        public AppSettings GetSettings() => _appSettings;

        private const string AppName = "DesktopApplicationTemplate";
        private const string RegistryKeyPath = @"Software\\Microsoft\\Windows\\CurrentVersion\\Run";

        private static void EnableAutoStart()
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true)
                                     ?? Registry.CurrentUser.CreateSubKey(RegistryKeyPath)!;

            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(exePath))
            {
                key.SetValue(AppName, $"\"{exePath}\"");
            }
        }

        private static void DisableAutoStart()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            key?.DeleteValue(AppName, false);
        }

        private static bool IsAutoStartEnabled()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            string? value = key?.GetValue(AppName) as string;
            return !string.IsNullOrEmpty(value);
        }
    }
}
