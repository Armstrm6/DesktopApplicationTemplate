using System;
using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using LogLevel = DesktopApplicationTemplate.Core.Services.LogLevel;
using Microsoft.VisualBasic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DesktopApplicationTemplate.UI.Helpers;
using System.Linq;
using System.Windows.Media;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Views;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

namespace DesktopApplicationTemplate.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly ILogger<MainView>? _logger;

        public MainView(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            var factory = App.AppHost.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            _logger = factory?.CreateLogger<MainView>();
            DataContext = _viewModel;
            _viewModel.EditRequested += OnEditRequested;
            KeyDown += MainView_KeyDown;
            MouseDown += MainView_MouseDown;
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseCommand_Executed));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeCommand_Executed));
            Closing += (_, _) => _logger?.LogInformation("MainView closing");
            ShowHome();
        }

        public void ShowHome()
        {
            ContentFrame.Content = null;
            ContentFrame.Visibility = Visibility.Collapsed;
            HomeContentGrid.Visibility = Visibility.Visible;
        }

        private void ShowPage(Page page)
        {
            HomeContentGrid.Visibility = Visibility.Collapsed;
            ContentFrame.Visibility = Visibility.Visible;
            ContentFrame.Content = page;
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _logger?.LogInformation("Close command invoked");
            Close();
        }

        private void MinimizeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _logger?.LogInformation("Minimize command invoked");
            SystemCommands.MinimizeWindow(this);
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            _logger?.LogInformation("Home button clicked");
            _viewModel.SelectedService = null;
            ShowHome();
        }

        private Page? GetOrCreateServicePage(ServiceViewModel svc)
        {
            if (svc.ServicePage != null)
                return svc.ServicePage;

            svc.ServicePage = svc.ServiceType switch
            {
                "TCP" => App.AppHost.Services.GetRequiredService<TcpServiceView>(),
                "HTTP" => App.AppHost.Services.GetRequiredService<HttpServiceView>(),
                "File Observer" => App.AppHost.Services.GetRequiredService<FileObserverView>(),
                "HID" => App.AppHost.Services.GetRequiredService<HidViews>(),
                "Heartbeat" => App.AppHost.Services.GetRequiredService<HeartbeatView>(),
                "SCP" => App.AppHost.Services.GetRequiredService<SCPServiceView>(),
                "MQTT" => App.AppHost.Services.GetRequiredService<MQTTServiceView>(),
                "FTP" => App.AppHost.Services.GetRequiredService<FTPServiceView>(),
                _ => null
            };

            if (svc.ServicePage != null)
            {
                if (svc.ServicePage.DataContext is ILoggingViewModel vm && vm.Logger is LoggingService logger)
                {
                    if (vm.Logger != null)
                    {
                        logger.LogAdded += entry => svc.AddLog(entry.Message, entry.Color, entry.Level);
                    }
                }

                if (svc.ServicePage.DataContext is INetworkAwareViewModel navm)
                {
                    navm.UpdateNetworkConfiguration(_viewModel.NetworkConfig.CurrentConfiguration);
                }
            }

            return svc.ServicePage;
        }

        private void AddService_Click(object sender, RoutedEventArgs e)
        {
            _logger?.LogDebug("AddService button clicked");
            var existing = _viewModel.Services.Select(s => s.DisplayName.Split(" - ").Last());
            var vm = new CreateServiceViewModel(existing);
            var window = new CreateServiceWindow(vm);

            if (window.ShowDialog() == true)
            {
                var name = window.CreatedServiceName;
                var type = window.CreatedServiceType;

                var newService = new ServiceViewModel
                {
                    DisplayName = $"{type} - {name}",
                    ServiceType = type,
                    IsActive = false
                };

                newService.SetColorsByType();
                newService.LogAdded += _viewModel.OnServiceLogAdded;
                newService.ActiveChanged += _viewModel.OnServiceActiveChanged;

                GetOrCreateServicePage(newService);

                _viewModel.Services.Add(newService);
                _logger?.LogInformation("Service {Name} added", newService.DisplayName);
                _viewModel.SelectedService = newService;
                ServiceList.ScrollIntoView(newService);

                if (type == "MQTT" && newService.ServicePage is MQTTServiceView mqttView)
                {
                    var vm = (MqttServiceViewModel)mqttView.DataContext!;
                    newService.ActiveChanged += async active =>
                    {
                        if (active)
                            await vm.ConnectAsync();
                    };
                }

                if (type == "CSV Creator")
                {
                    var csvVm = App.AppHost.Services.GetRequiredService<CsvViewerViewModel>();
                    var csvWindow = new CsvViewerWindow(csvVm);
                    csvVm.RequestClose += () => csvWindow.Close();
                    csvWindow.ShowDialog();
                }
                else if (newService.ServicePage != null)
                {
                    ShowPage(newService.ServicePage);
                }
                _viewModel.SaveServices();
                _logger?.LogDebug("AddService workflow completed");
            }
        }

        private void OnEditRequested(ServiceViewModel service)
        {
            _logger?.LogDebug("Edit requested for {Name}", service.DisplayName);
            var page = GetOrCreateServicePage(service);
            if (page != null)
            {
                ShowPage(page);
                _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
            }
        }

        private void RemoveService_Click(object sender, RoutedEventArgs e)
        {
            _logger?.LogDebug("RemoveService button clicked");
            if (DataContext is ViewModels.MainViewModel vm &&
                vm.RemoveServiceCommand.CanExecute(null))
            {
                vm.RemoveServiceCommand.Execute(null);
                _logger?.LogDebug("RemoveService command executed");
            }
        }
        private void EditService_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedService == null)
                return;

            _logger?.LogDebug("EditService button clicked for {Name}", _viewModel.SelectedService.DisplayName);
            OpenServiceEditor(_viewModel.SelectedService);
        }

        private void ServiceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _logger?.LogDebug("Service selection changed");
            if (_viewModel.SelectedService != null)
            {
                var page = GetOrCreateServicePage(_viewModel.SelectedService);
                if (page != null)
                {
                    ShowPage(page);
                }
            }
            else
            {
                ShowHome();
            }
        }

        private void OpenCsvViewer_Click(object sender, RoutedEventArgs e)
        {
            var vm = App.AppHost.Services.GetRequiredService<CsvViewerViewModel>();
            var window = new CsvViewerWindow(vm);
            vm.RequestClose += () => window.Close();
            window.ShowDialog();
        }

        private void OpenFilter_Click(object sender, RoutedEventArgs e)
        {
            var window = new FilterWindow { DataContext = _viewModel.Filters };
            if (window.ShowDialog() == true)
            {
                // filters already applied via PropertyChanged event
            }
        }

        private void EditServiceMenu_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is ServiceViewModel svc)
            {
                OpenServiceEditor(svc);
            }
        }

        private void DeleteServiceMenu_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is ServiceViewModel svc)
            {
                var index = _viewModel.Services.IndexOf(svc);
                svc.LogAdded -= _viewModel.OnServiceLogAdded;
                _viewModel.Services.Remove(svc);
                if (_viewModel.Services.Count > 0)
                {
                    if (index >= _viewModel.Services.Count) index = _viewModel.Services.Count - 1;
                    _viewModel.SelectedService = _viewModel.Services[index];
                }
                else
                {
                    _viewModel.SelectedService = null;
                }
                _viewModel.SaveServices();
            }
        }

        private void RenameServiceMenu_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is ServiceViewModel svc)
            {
                string input = Interaction.InputBox("Enter new service name:", "Rename Service", svc.DisplayName);
                if (!string.IsNullOrWhiteSpace(input))
                {
                    var namePart = input.Contains(" - ") ? input.Split(" - ").Last() : input;
                    if (_viewModel.Services.Any(s => s != svc && s.DisplayName.Split(" - ").Last().Equals(namePart, StringComparison.OrdinalIgnoreCase)))
                    {
                        namePart = _viewModel.GenerateServiceName(svc.ServiceType);
                    }
                    svc.DisplayName = $"{svc.ServiceType} - {namePart}";
                    _viewModel.SaveServices();
                }
            }
        }

        private void ChangeColorMenu_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is ServiceViewModel svc)
            {
                var dlg = new ColorPickerWindow { Owner = this };
                if (dlg.ShowDialog() == true)
                {
                    var color = dlg.ChosenColor;
                    var brush = new SolidColorBrush(color);
                    foreach (var s in _viewModel.Services.Where(s => s.ServiceType == svc.ServiceType))
                    {
                        s.BackgroundColor = brush;
                        s.BorderColor = brush;
                    }
                    _viewModel.SaveServices();
                }
            }
        }

        private void MainView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                _viewModel.SelectedService = null;
                ShowHome();
            }
        }

        private void HeaderBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                var element = e.OriginalSource as DependencyObject;
                if (Helpers.VisualTreeHelperExtensions.FindParent<ButtonBase>(element) == null)
                {
                    try
                    {
                        DragMove();
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger?.LogWarning(ex, "DragMove failed");
                    }
                }
                e.Handled = true;
            }
        }

        private void HeaderBar_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as DependencyObject;
            if (Helpers.VisualTreeHelperExtensions.FindParent<ButtonBase>(element) != null)
                return;

            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

            _logger?.LogInformation("Window state changed to {State}", WindowState);
            e.Handled = true;
        }

        private void MainView_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as DependencyObject;
            bool clickedListItem = Helpers.VisualTreeHelperExtensions.FindParent<ListBoxItem>(element) != null;
            bool clickedFrame = Helpers.VisualTreeHelperExtensions.FindParent<Frame>(element) != null;
            bool clickedButton = Helpers.VisualTreeHelperExtensions.FindParent<ButtonBase>(element) != null;

            if (e.ChangedButton == System.Windows.Input.MouseButton.Left && !clickedListItem && !clickedFrame && !clickedButton)
            {
                try
                {
                    DragMove();
                }
                catch (InvalidOperationException ex)
                {
                    _logger?.LogWarning(ex, "DragMove failed");
                }
            }

            if (_viewModel.SelectedService != null && !clickedListItem && !clickedFrame)
            {
                _viewModel.SelectedService = null;
                ShowHome();
            }
        }

        private void ServiceItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
                return;

            if ((sender as Border)?.DataContext is ServiceViewModel svc)
            {
                _logger?.LogDebug("Service {Name} double-clicked", svc.DisplayName);
                OpenServiceEditor(svc);
            }
        }

        private void OpenServiceEditor(ServiceViewModel svc)
        {
            if (svc.ServiceType == "CSV Creator")
            {
                var vm = App.AppHost.Services.GetRequiredService<CsvViewerViewModel>();
                var window = new CsvViewerWindow(vm);
                vm.RequestClose += () => window.Close();
                window.ShowDialog();
            }
            else
            {
                var page = GetOrCreateServicePage(svc);
                if (page != null)
                {
                    svc.IsActive = false;
                    ShowPage(page);
                    _logger?.LogDebug("EditService workflow completed for {Name}", svc.DisplayName);
                }
            }
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            var page = App.AppHost.Services.GetRequiredService<SettingsPage>();
            ShowPage(page);
        }

        private System.Windows.Point _dragStart;
        private void ServiceItem_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(null);
        }

        private void ServiceItem_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
                return;

            var position = e.GetPosition(null);
            if (Math.Abs(position.X - _dragStart.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(position.Y - _dragStart.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (sender is Border border && border.DataContext is ServiceViewModel svc)
                {
                    DragDrop.DoDragDrop(border, svc, System.Windows.DragDropEffects.Move);
                }
            }
        }

        private void ServiceItem_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(ServiceViewModel)))
                return;

            var source = (ServiceViewModel)e.Data.GetData(typeof(ServiceViewModel))!;
            var target = (sender as Border)?.DataContext as ServiceViewModel;
            if (source == null || target == null || source == target)
                return;

            int oldIndex = _viewModel.Services.IndexOf(source);
            int newIndex = _viewModel.Services.IndexOf(target);
            if (oldIndex != newIndex)
            {
                _viewModel.Services.Move(oldIndex, newIndex);
                _viewModel.SaveServices();
            }
        }

        private void GlobalLogLevelBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel == null)
            {
                _logger?.LogWarning("Log level changed before view model initialization");
                return;
            }

            if (GlobalLogLevelBox.SelectedItem is ComboBoxItem item && item.Content != null)
            {
                _viewModel.LogLevelFilter = item.Content.ToString() switch
                {
                    "Warning" => LogLevel.Warning,
                    "Error" => LogLevel.Error,
                    "Debug" => LogLevel.Debug,
                    _ => LogLevel.Debug
                };

                _logger?.LogInformation("Global log level set to {Level}", _viewModel.LogLevelFilter);
            }
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            _logger?.LogInformation("Clear log button clicked");
            _viewModel.ClearLogs();
        }

        private void ExportLog_Click(object sender, RoutedEventArgs e)
        {
            _logger?.LogInformation("Export log button clicked");
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "exported_logs.txt");
            _viewModel.ExportDisplayedLogs(path);
            _logger?.LogInformation("Logs exported to {Path}", path);
        }

        private void RefreshLog_Click(object sender, RoutedEventArgs e)
        {
            _logger?.LogDebug("Refresh log button clicked");
            _viewModel.RefreshLogs();
        }

    }
}
