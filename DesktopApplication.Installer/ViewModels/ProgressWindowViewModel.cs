using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplication.Installer.Helpers;

namespace DesktopApplication.Installer.ViewModels
{
    internal class ProgressWindowViewModel : INotifyPropertyChanged
    {
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

        public ProgressWindowViewModel(string installPath, bool firewall, bool startup)
        {
            _installPath = installPath;
            _firewall = firewall;
            _startup = startup;
            CancelCommand = new RelayCommand(() => _cts.Cancel());
        }

        public async Task StartAsync()
        {
            var logFile = Path.Combine(_installPath, "install_log.txt");
            Directory.CreateDirectory(_installPath);
            using var writer = new StreamWriter(logFile, append: false);
            try
            {
                await RunStep(writer, 10, "Creating directories...", async () => await Task.Delay(500));
                await RunStep(writer, 30, "Copying files...", async () => await Task.Delay(500));
                await RunStep(writer, 60, "Installing dependencies...", async () => await Task.Delay(500));
                if (_firewall)
                    await RunStep(writer, 80, "Configuring firewall...", async () => await Task.Delay(500));
                if (_startup)
                    await RunStep(writer, 90, "Configuring autostart...", async () => await Task.Delay(500));
                await RunStep(writer, 100, "Finished.", async () => await Task.Delay(200));
            }
            catch (OperationCanceledException)
            {
                AppendLog(writer, "Installation cancelled.");
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
            await action();
            Progress = progress;
        }

        private void AppendLog(StreamWriter writer, string message)
        {
            writer.WriteLine(message);
            writer.Flush();
            LogText += message + Environment.NewLine;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
