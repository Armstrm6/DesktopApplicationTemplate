using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.Services.Ftp.UI.ViewModels.Ftp.Edit;

namespace DesktopApplicationTemplate.Services.Ftp.UI.Views.Ftp.Edit;

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
