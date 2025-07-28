using System.Collections.ObjectModel;
using System.Windows.Input;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class HidViewModel : ViewModelBase
    {
        private string _messageTemplate = string.Empty;
        public string MessageTemplate
        {
            get => _messageTemplate;
            set { _messageTemplate = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> AvailableServices { get; } = new();

        private string _attachedService = string.Empty;
        public string AttachedService
        {
            get => _attachedService;
            set { _attachedService = value; OnPropertyChanged(); Logger?.Log($"Attached service set to {value}", LogLevel.Debug); }
        }

        public ObservableCollection<string> UsbProtocols { get; } = new() { "2.0", "3.0" };

        private string _selectedUsbProtocol = "2.0";
        public string SelectedUsbProtocol
        {
            get => _selectedUsbProtocol;
            set { _selectedUsbProtocol = value; OnPropertyChanged(); Logger?.Log($"USB protocol set to {value}", LogLevel.Debug); }
        }

        private string _debounceTimeMs = "0";
        public string DebounceTimeMs
        {
            get => _debounceTimeMs;
            set { _debounceTimeMs = value; OnPropertyChanged(); Logger?.Log($"Debounce time set to {value}ms", LogLevel.Debug); }
        }

        private string _keyDownTimeMs = "0";
        public string KeyDownTimeMs
        {
            get => _keyDownTimeMs;
            set { _keyDownTimeMs = value; OnPropertyChanged(); Logger?.Log($"Key down time set to {value}ms", LogLevel.Debug); }
        }

        private string _formatTemplate = "{0}";
        public string FormatTemplate
        {
            get => _formatTemplate;
            set { _formatTemplate = value; OnPropertyChanged(); }
        }

        public ILoggingService? Logger { get; set; }

        private string _finalMessage = string.Empty;
        public string FinalMessage
        {
            get => _finalMessage;
            set { _finalMessage = value; OnPropertyChanged(); }
        }

        public ICommand BuildCommand { get; }
        public ICommand SaveCommand { get; }

        public HidViewModel()
        {
            BuildCommand = new RelayCommand(BuildMessage);
            SaveCommand = new RelayCommand(Save);
        }

        private void BuildMessage()
        {
            Logger?.Log("Building HID message", LogLevel.Debug);
            FinalMessage = string.Format(FormatTemplate ?? "{0}", MessageTemplate);
            Logger?.Log($"Final HID message: {FinalMessage}", LogLevel.Debug);
            if (!string.IsNullOrWhiteSpace(AttachedService))
            {
                Logger?.Log($"Forwarding message to {AttachedService}", LogLevel.Debug);
                MessageForwarder.Forward(AttachedService, FinalMessage);
            }
        }

        private void Save()
        {
            Logger?.Log("Saving HID configuration", LogLevel.Debug);
            SaveConfirmationHelper.Show();
        }
    }
}
