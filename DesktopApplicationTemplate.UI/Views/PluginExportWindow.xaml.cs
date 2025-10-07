using System;
using System.Windows;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.Win32;

namespace DesktopApplicationTemplate.UI.Views;

/// <summary>
/// Interaction logic for PluginExportWindow.xaml
/// </summary>
public partial class PluginExportWindow : Window
{
    private readonly PluginExportViewModel _viewModel;

    public PluginExportWindow(PluginExportViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;
        _viewModel.ExportCompleted += OnExportCompleted;
        _viewModel.CancelRequested += OnCancelRequested;
        Closed += OnClosed;
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Peak IoT Plug-in (*.peakiot)|*.peakiot|All files (*.*)|*.*",
            FileName = string.IsNullOrWhiteSpace(_viewModel.DestinationPath)
                ? _viewModel.SuggestedFileName
                : _viewModel.DestinationPath,
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.DestinationPath = dialog.FileName;
        }
    }

    private void OnExportCompleted(object? sender, PluginExportResult result)
    {
        if (result.Success)
        {
            MessageBox.Show(this, result.Message, "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
            return;
        }

        MessageBox.Show(this, result.Message, "Export Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void OnCancelRequested(object? sender, EventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _viewModel.ExportCompleted -= OnExportCompleted;
        _viewModel.CancelRequested -= OnCancelRequested;
        _viewModel.Dispose();
        Closed -= OnClosed;
    }
}
