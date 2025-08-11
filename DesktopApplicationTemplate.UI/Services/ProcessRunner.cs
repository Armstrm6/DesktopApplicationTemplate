using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    public class ProcessRunner : IProcessRunner
    {
        public async Task RunAsync(string fileName, string arguments, CancellationToken cancellationToken = default)
        {
            var startInfo = new ProcessStartInfo(fileName, arguments)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
                throw new InvalidOperationException($"Command '{fileName} {arguments}' failed: {error}");
            }
        }
    }
}
