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

        public StartupService(IConfiguration configuration)
        {
            _configuration = configuration;
            _appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
        }


        public async Task RunStartupChecksAsync()
        {
            Console.WriteLine($"[Startup] Environment: {_appSettings.Environment}");
            await Task.Run(() => DependencyChecker.CheckAll());

            if (_appSettings.AutoStart)
            {
                AutoStartHelper.EnableAutoStart();
            }
            else
            {
                AutoStartHelper.DisableAutoStart();
            }
        }

        public AppSettings GetSettings() => _appSettings;
    }
}
