using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class ServiceEditorWindow : Window
    {
        public ServiceEditorWindow(Page servicePage)
        {
            InitializeComponent();
            EditorFrame.Content = servicePage;
            SaveConfirmationHelper.SaveConfirmed += OnSaveConfirmed;
            Closed += (s, e) => SaveConfirmationHelper.SaveConfirmed -= OnSaveConfirmed;
            Closing += OnClosing;
            if (servicePage.DataContext is ILoggingViewModel vm)
            {
                CloseEditorConfirmationHelper.Logger = vm.Logger;
            }
        }

        private void OnSaveConfirmed()
        {
            Close();
        }

        private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CloseEditorConfirmationHelper.Show())
            {
                e.Cancel = true;
            }
        }
    }
}
