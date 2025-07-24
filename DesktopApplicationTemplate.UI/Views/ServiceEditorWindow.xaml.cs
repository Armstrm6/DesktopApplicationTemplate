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
        }

        private void OnSaveConfirmed()
        {
            Close();
        }
    }
}
