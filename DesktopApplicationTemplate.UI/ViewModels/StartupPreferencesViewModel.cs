using System;
using System.Windows.Input;
using DesktopApplicationTemplate.UI.Helpers;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class StartupPreferencesViewModel : ViewModelBase
    {
        private bool _runServicesOnStartup;
        private bool _runUiOnStartup;

        public StartupPreferencesViewModel(bool runServicesOnStartup, bool runUiOnStartup)
        {
            _runServicesOnStartup = runServicesOnStartup;
            _runUiOnStartup = runUiOnStartup;
            ConfirmCommand = new RelayCommand(() => RequestClose?.Invoke(this, true));
            CancelCommand = new RelayCommand(() => RequestClose?.Invoke(this, false));
        }

        public bool RunServicesOnStartup
        {
            get => _runServicesOnStartup;
            set
            {
                if (_runServicesOnStartup == value)
                {
                    return;
                }

                _runServicesOnStartup = value;
                OnPropertyChanged();
            }
        }

        public bool RunUIOnStartup
        {
            get => _runUiOnStartup;
            set
            {
                if (_runUiOnStartup == value)
                {
                    return;
                }

                _runUiOnStartup = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConfirmCommand { get; }

        public ICommand CancelCommand { get; }

        public event EventHandler<bool>? RequestClose;
    }
}

