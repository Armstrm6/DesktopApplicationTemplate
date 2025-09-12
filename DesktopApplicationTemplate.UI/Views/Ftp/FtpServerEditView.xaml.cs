using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels.Ftp;

namespace DesktopApplicationTemplate.UI.Views.Ftp;

/// <summary>
/// Interaction logic for FtpServerEditView.
/// </summary>
public partial class FtpServerEditView : Page
{
    public FtpServerEditView(FtpServerEditViewModel viewModel)
    {
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        InitializeComponent();
    }
}
