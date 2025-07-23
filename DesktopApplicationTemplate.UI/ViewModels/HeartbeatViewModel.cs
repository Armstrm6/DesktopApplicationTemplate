using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class HeartbeatViewModel : INotifyPropertyChanged
    {
        private string _baseMessage = "HEARTBEAT";
        public string BaseMessage
        {
            get => _baseMessage;
            set { _baseMessage = value; OnPropertyChanged(); }
        }

        private bool _includePing;
        public bool IncludePing
        {
            get => _includePing;
            set { _includePing = value; OnPropertyChanged(); }
        }

        private bool _includeStatus;
        public bool IncludeStatus
        {
            get => _includeStatus;
            set { _includeStatus = value; OnPropertyChanged(); }
        }

        private string _finalMessage = string.Empty;
        public string FinalMessage
        {
            get => _finalMessage;
            set { _finalMessage = value; OnPropertyChanged(); }
        }

        public ICommand BuildCommand { get; }

        public HeartbeatViewModel()
        {
            BuildCommand = new RelayCommand(BuildMessage);
        }

        private void BuildMessage()
        {
            var msg = BaseMessage;
            if (IncludePing)
            {
                msg += " | PING";
            }
            if (IncludeStatus)
            {
                msg += " | STATUS";
            }
            FinalMessage = msg;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
