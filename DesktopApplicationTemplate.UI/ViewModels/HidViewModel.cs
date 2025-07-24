using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Input;
using DesktopApplicationTemplate.UI.Helpers;

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

        private void BuildMessage() => FinalMessage = MessageTemplate;

        private void Save() => SaveConfirmationHelper.Show();
    }
}
