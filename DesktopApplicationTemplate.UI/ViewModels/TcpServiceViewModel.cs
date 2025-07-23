using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class TcpServiceViewModel : INotifyPropertyChanged
    {
        private string _statusMessage;
        private bool _isServerRunning;

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsServerRunning
        {
            get => _isServerRunning;
            set
            {
                _isServerRunning = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }

        public TcpServiceViewModel()
        {
            StatusMessage = "Chappie is initializing...";
            IsServerRunning = false;
            SaveCommand = new RelayCommand(Save);
        }

        private void Save()
        {
            MessageBox.Show("Configuration saved.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
