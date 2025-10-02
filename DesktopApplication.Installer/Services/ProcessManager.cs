using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DesktopApplication.Installer.Services
{
    internal interface IProcessManager
    {
        IEnumerable<int> GetProcessIdsByName(string processName);
        void KillProcess(int processId);
    }

    internal class ProcessManager : IProcessManager
    {
        public IEnumerable<int> GetProcessIdsByName(string processName)
            => Process.GetProcessesByName(processName).Select(p => p.Id);

        public void KillProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // ignore failures stopping processes
            }
        }
    }
}
