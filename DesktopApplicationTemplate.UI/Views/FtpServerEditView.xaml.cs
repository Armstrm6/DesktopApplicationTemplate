using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views;

/// <summary>
/// Interaction logic for FtpServerEditView.
/// </summary>
public partial class FtpServerEditView : Page
{
    public FtpServerEditView(FtpServerEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }
}
