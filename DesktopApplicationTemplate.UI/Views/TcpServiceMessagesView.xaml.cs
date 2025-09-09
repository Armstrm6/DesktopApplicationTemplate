using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    /// <summary>
    /// Interaction logic for TcpServiceMessagesView.xaml
    /// </summary>
    public partial class TcpServiceMessagesView : Page, IServiceLogHost
    {
        public TcpServiceMessagesView(TcpServiceMessagesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public void SetServiceContext(ServiceViewModel service)
        {
            LogView.DataContext = new ServiceLogViewModel(service.DisplayName, service.ServiceType, service.Logs);
            if (DataContext is TcpServiceMessagesViewModel vm)
                vm.SetService(service);
        }

        private void EditScript_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not TcpServiceMessagesViewModel vm)
                return;

            var editor = new ScriptEditorWindow();
            if (editor.DataContext is ScriptEditorViewModel svm)
            {
                svm.ScriptText = vm.Script;
                svm.TestMessage = vm.TestMessage;
            }

            if (editor.ShowDialog() == true)
            {
                vm.Script = editor.ScriptText;
                vm.TestMessage = editor.LastTestMessage;
                vm.Save();
            }
        }
    }
}
