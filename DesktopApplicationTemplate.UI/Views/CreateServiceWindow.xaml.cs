using System.Windows;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class CreateServiceWindow : Window
    {
        public string CreatedServiceName { get; private set; } = string.Empty;
        public string CreatedServiceType { get; private set; } = string.Empty;

        public CreateServiceWindow(CreateServiceViewModel viewModel)
        {
            InitializeComponent();
            var page = new CreateServicePage(viewModel);
            page.ServiceCreated += (name, type) =>
            {
                CreatedServiceName = name;
                CreatedServiceType = type;
                DialogResult = true;
            };
            page.Cancelled += () =>
            {
                DialogResult = false;
            };
            ContentFrame.Content = page;
        }
    }
}
