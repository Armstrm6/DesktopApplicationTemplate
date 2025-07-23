using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Models
{
    public class AppSettings
    {
        public string Environment { get; set; } = "Development";
        public string ServerIP { get; set; } = "127.0.0.1";
        public int ServerPort { get; set; } = 9000;
        public string LogLevel { get; set; } = "Debug";
        public bool AutoStart { get; set; } = true;
        public string DefaultCSharpScriptPath { get; set; } = string.Empty;

        // Add more settings as needed
    }
}
