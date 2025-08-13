using DesktopApplicationTemplate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    public interface IStartupService
    {
        Task RunStartupChecksAsync();
        AppSettings GetSettings();
    }

}
