using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Input;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class HidViewModel
    {
        public ICommand SaveCommand { get; }

        public HidViewModel()
        {
            SaveCommand = new RelayCommand(Save);
        }

        private void Save()
        {
            MessageBox.Show("Configuration saved.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
