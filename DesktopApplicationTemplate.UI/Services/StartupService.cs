using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                AutoStartHelper.EnableAutoStart();
                _logger?.Log("Auto-start enabled", LogLevel.Debug);
            }
            else
            {
                AutoStartHelper.DisableAutoStart();
                _logger?.Log("Auto-start disabled", LogLevel.Debug);
            }
        }

        public AppSettings GetSettings() => _appSettings;
    }
}
