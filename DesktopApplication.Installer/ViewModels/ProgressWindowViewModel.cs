using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplication.Installer.Helpers;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplication.Installer.ViewModels
{
    internal class ProgressWindowViewModel : INotifyPropertyChanged
    {
        private readonly ILoggingService? _logger;
        private readonly CancellationTokenSource _cts = new();
        private int _progress;
        private string _logText = string.Empty;

        public int Progress
        {
            get => _progress;
            private set { _progress = value; OnPropertyChanged(); }
        }

        public string LogText
        {
            get => _logText;
            private set { _logText = value; OnPropertyChanged(); }
        }

        public ICommand CancelCommand { get; }

        public event Action? Completed;

        private readonly string _installPath;
        private readonly bool _firewall;
        private readonly bool _startup;

        public ProgressWindowViewModel(string installPath, bool firewall, bool startup, ILoggingService? logger = null)
        {
            _installPath = installPath;
            _firewall = firewall;
            _startup = startup;
            _logger = logger;
            CancelCommand = new RelayCommand(() => _cts.Cancel());
            _logger?.Log("Progress window initialized", LogLevel.Debug);
        }

        public async Task StartAsync()
        {
            var logFile = Path.Combine(_installPath, "install_log.txt");
            Directory.CreateDirectory(_installPath);
            using var writer = new StreamWriter(logFile, append: false);
            try
            {
                _logger?.Log("Installation started", LogLevel.Debug);
                await RunStep(writer, 10, "Creating directories...", async () => await Task.Run(() => Directory.CreateDirectory(_installPath)));
                await RunStep(writer, 30, "Copying files...", async () => await Task.Run(CopyApplicationFiles));
                await RunStep(writer, 60, "Installing dependencies...", async () => await Task.Delay(500));
                if (_firewall)
                    await RunStep(writer, 80, "Configuring firewall...", async () => await Task.Delay(500));
                if (_startup)
                    await RunStep(writer, 90, "Configuring autostart...", async () => await Task.Delay(500));
                await RunStep(writer, 100, "Finished.", async () => await Task.Delay(200));
                _logger?.Log("Installation completed", LogLevel.Debug);
            }
            catch (OperationCanceledException)
            {
                AppendLog(writer, "Installation cancelled.");
                _logger?.Log("Installation cancelled", LogLevel.Warning);
            }
            finally
            {
                writer.Flush();
                Completed?.Invoke();
            }
        }

        private async Task RunStep(StreamWriter writer, int progress, string message, Func<Task> action)
        {
            _cts.Token.ThrowIfCancellationRequested();
            AppendLog(writer, message);
            _logger?.Log(message, LogLevel.Debug);
            await action();
            Progress = progress;
        }

        private void AppendLog(StreamWriter writer, string message)
        {
            writer.WriteLine(message);
            writer.Flush();
            LogText += message + Environment.NewLine;
            _logger?.Log(message, LogLevel.Debug);
        }

        private void CopyApplicationFiles()
        {
            var sourceDir = AppContext.BaseDirectory;
            _logger?.Log($"Copying from {sourceDir}", LogLevel.Debug);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var dest = Path.Combine(_installPath, Path.GetFileName(file));
                File.Copy(file, dest, overwrite: true);
            }

            var depsJson = Directory.GetFiles(sourceDir, "*.deps.json").FirstOrDefault();
            if (depsJson is not null)
            {
                foreach (var relative in GetRuntimeDependencies(depsJson))
                {
                    var file = Path.Combine(sourceDir, relative);
                    if (!File.Exists(file))
                        continue;
                    var dest = Path.Combine(_installPath, Path.GetFileName(file));
                    File.Copy(file, dest, overwrite: true);
                    _logger?.Log($"Copied runtime dependency {file} -> {dest}", LogLevel.Debug);
                }
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var destDir = Path.Combine(_installPath, Path.GetFileName(dir));
                CopyDirectory(dir, destDir);
            }
        }

        private void CopyDirectory(string source, string dest)
        {
            _logger?.Log($"Copying directory {source} -> {dest}", LogLevel.Debug);
            Directory.CreateDirectory(dest);
            foreach (var file in Directory.GetFiles(source))
            {
                var destFile = Path.Combine(dest, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
                _logger?.Log($"Copied {file} -> {destFile}", LogLevel.Debug);
            }
            foreach (var dir in Directory.GetDirectories(source))
            {
                var destSub = Path.Combine(dest, Path.GetFileName(dir));
                _logger?.Log($"Descending into {dir}", LogLevel.Debug);
                CopyDirectory(dir, destSub);
            }
        }

        internal static IEnumerable<string> GetRuntimeDependencies(string depsJsonPath)
            => ParseRuntimeDependencies(File.ReadAllText(depsJsonPath));

        internal static IEnumerable<string> ParseRuntimeDependencies(string depsJsonContent)
        {
            var files = new HashSet<string>();
            using var doc = JsonDocument.Parse(depsJsonContent);
            if (doc.RootElement.TryGetProperty("targets", out var targets))
            {
                foreach (var framework in targets.EnumerateObject())
                {
                    foreach (var library in framework.Value.EnumerateObject())
                    {
                        if (library.Value.TryGetProperty("runtime", out var runtime))
                        {
                            foreach (var runtimeFile in runtime.EnumerateObject())
                                files.Add(runtimeFile.Name);
                        }
                        if (library.Value.TryGetProperty("runtimeTargets", out var runtimeTargets))
                        {
                            foreach (var runtimeFile in runtimeTargets.EnumerateObject())
                                files.Add(runtimeFile.Name);
                        }
                    }
                }
            }

            return files.Select(f => f.Replace('/', Path.DirectorySeparatorChar));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
