using System.Windows;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class ScriptEditorWindow : Window
    {
        public string ScriptText { get; private set; } = string.Empty;

        public ScriptEditorWindow(string initial)
        {
            InitializeComponent();
            ScriptBox.Text = initial;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            ScriptText = ScriptBox.Text;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
