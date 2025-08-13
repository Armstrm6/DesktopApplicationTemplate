using System.ComponentModel;
using System.Windows;
using DesktopApplication.Installer.ViewModels;

namespace DesktopApplication.Installer.Views
{
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ProgressWindowViewModel oldVm)
                oldVm.PropertyChanged -= OnVmPropertyChanged;
            if (e.NewValue is ProgressWindowViewModel newVm)
                newVm.PropertyChanged += OnVmPropertyChanged;
        }

        private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProgressWindowViewModel.LogText))
            {
                LogScrollViewer.ScrollToEnd();
            }
        }
    }
}
