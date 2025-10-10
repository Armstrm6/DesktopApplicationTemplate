using DesktopApplicationTemplate.UI.ViewModels.Hid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesktopApplicationTemplate.UI.Views.Hid
{
    /// <summary>
    /// Interaction logic for HidView.xaml
    /// </summary>
    public partial class HidView : Page
    {
        private readonly HidViewModel _viewModel;

        public HidView(HidViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
        }
    }
}
