using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;
using DesktopApplicationTemplate.UI.Helpers;

namespace DesktopApplicationTemplate.UI.ViewModels.Heartbeat
{
    public class HeartbeatViewModel : ValidatableViewModelBase
    {
        private string _baseMessage = "HEARTBEAT";
        public string BaseMessage
        {
            get => _baseMessage;
            set
            {
                _baseMessage = value;
                if (string.IsNullOrWhiteSpace(value))
                {
                    AddError(nameof(BaseMessage), "Message cannot be empty");
                }
                else
                {
                    ClearErrors(nameof(BaseMessage));
                }
                OnPropertyChanged();
            }
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
        public ICommand SaveCommand { get; }

        private readonly SaveConfirmationHelper _saveHelper;
        private readonly IHeartbeatService _heartbeatService;

        public HeartbeatViewModel(SaveConfirmationHelper saveHelper, IHeartbeatService heartbeatService)
        {
            _saveHelper = saveHelper;
            _heartbeatService = heartbeatService ?? throw new ArgumentNullException(nameof(heartbeatService));
            BuildCommand = new RelayCommand(BuildMessage);
            SaveCommand = new RelayCommand(Save);
        }

        private void BuildMessage()
        {
            var options = new HeartbeatServiceOptions
            {
                BaseMessage = BaseMessage,
                IncludePing = IncludePing,
                IncludeStatus = IncludeStatus
            };

            FinalMessage = _heartbeatService.BuildHeartbeatMessage(options);
        }

        private void Save() => _saveHelper.Show();

        // OnPropertyChanged from ViewModelBase
    }
}
