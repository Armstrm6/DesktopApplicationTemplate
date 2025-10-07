using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Views.Shared
{
    /// <summary>
    /// Interaction logic for ServiceLogView.xaml
    /// </summary>
    public partial class ServiceLogView : UserControl
    {
        public ServiceLogView()
        {
            InitializeComponent();
        }

        private void CopyCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LogListBox?.SelectedItems?.Count > 0;
        }

        private void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (LogListBox?.SelectedItems is null || LogListBox.SelectedItems.Count == 0)
            {
                return;
            }

            var lines = LogListBox.SelectedItems
                .OfType<LogEntry>()
                .Select(entry => entry.Message)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            if (lines.Length == 0)
            {
                return;
            }

            try
            {
                Clipboard.SetText(string.Join(Environment.NewLine, lines));
            }
            catch (Exception)
            {
                // Clipboard.SetText can throw if the clipboard is unavailable; ignore and leave command silent.
            }
        }
    }
}
