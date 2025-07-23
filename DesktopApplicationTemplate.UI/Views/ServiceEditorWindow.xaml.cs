using System.Windows;
using System.Windows.Controls;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class ServiceEditorWindow : Window
    {
        public ServiceEditorWindow(Page servicePage)
        {
            InitializeComponent();
            EditorFrame.Content = servicePage;
        }
    }
}
