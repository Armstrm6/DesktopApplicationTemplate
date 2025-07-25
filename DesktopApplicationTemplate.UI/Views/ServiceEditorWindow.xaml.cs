using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.Helpers;

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
            CommandBindings.Add(new System.Windows.Input.CommandBinding(SystemCommands.CloseWindowCommand, (_, __) => Close()));
            Closing += ServiceEditorWindow_Closing;
        }

        private void OnSaveConfirmed()
        {
            Close();
        }

        private void ServiceEditorWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseConfirmationHelper.Logger?.Log("Service editor closing", Services.LogLevel.Debug);
            if (!CloseConfirmationHelper.Show())
            {
                e.Cancel = true;
                CloseConfirmationHelper.Logger?.Log("Close canceled", Services.LogLevel.Debug);
            }
        }
    }
}
