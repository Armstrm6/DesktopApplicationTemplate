using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DesktopApplicationTemplate.UI.Views.Shared
{
    /// <summary>
    /// Interaction logic for ServiceMessageTableView.xaml
    /// </summary>
    public partial class ServiceMessageTableView : UserControl
    {
        public ServiceMessageTableView()
        {
            InitializeComponent();
        }

        private void ColumnHeaderPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGridColumnHeader header || header.Column is null)
            {
                return;
            }

            var column = header.Column;

            column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            column.Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells);

            e.Handled = true;
        }
    }
}
