using System;
using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using LogLevel = DesktopApplicationTemplate.Core.Services.LogLevel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using DesktopApplicationTemplate.UI.Helpers;
using System.Linq;
using System.Windows.Media;
using DesktopApplicationTemplate.Models;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using DesktopApplicationTemplate.UI;

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
            _viewModel.AddServiceRequested += OnAddServiceRequested;
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
                "TCP" => App.AppHost.Services.GetRequiredService<TcpServiceMessagesView>(),
                "HTTP" => App.AppHost.Services.GetRequiredService<HttpServiceView>(),
                "File Observer" => App.AppHost.Services.GetRequiredService<FileObserverView>(),
                "HID" => App.AppHost.Services.GetRequiredService<HidViews>(),
                "Heartbeat" => App.AppHost.Services.GetRequiredService<HeartbeatView>(),
                "SCP" => App.AppHost.Services.GetRequiredService<SCPServiceView>(),
                "MQTT" => App.AppHost.Services.GetRequiredService<MqttTagSubscriptionsView>(),
                "FTP" => App.AppHost.Services.GetRequiredService<FTPServiceView>(),
                "FTP Server" => App.AppHost.Services.GetRequiredService<FtpServerView>(),
                "CSV Creator" => App.AppHost.Services.GetRequiredService<CsvServiceView>(),
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

                if (svc.ServiceType == "TCP" && svc.ServicePage.DataContext is TcpServiceMessagesViewModel tcpVm)
                {
                    tcpVm.AdvancedSettingsRequested += (_, _) =>
                    {
                        var settingsView = App.AppHost.Services.GetRequiredService<TcpServiceView>();
                        if (settingsView.DataContext is TcpServiceViewModel tvm)
                        {
                            tvm.RequestClose += (_, _) =>
                            {
                                if (svc.ServicePage != null)
                                    ShowPage(svc.ServicePage);
                                _viewModel.SaveServices();
                            };
                        }
                        ShowPage(settingsView);
                    };
                }
            }

            return svc.ServicePage;
        }

        private void AddService_Click(object sender, RoutedEventArgs e)
        {
            _logger?.LogDebug("AddService button clicked");
            if (_viewModel.AddServiceCommand.CanExecute(null))
            {
                _viewModel.AddServiceCommand.Execute(null);
            }
        }

        private void OnAddServiceRequested()
        {
            var window = App.AppHost.Services.GetRequiredService<CreateServiceWindow>();
            window.Owner = this;
            if (window.ShowDialog() == true)
            {
                if (window.CreatedServiceType == "MQTT")
                {
                    AddMqttService(window.CreatedServiceName, window.MqttOptions ?? new MqttServiceOptions());
                }
                else if (window.CreatedServiceType == "TCP")
                {
                    var svc = new ServiceViewModel
                    {
                        DisplayName = $"TCP - {window.CreatedServiceName}",
                        ServiceType = "TCP",
                        IsActive = false,
                        TcpOptions = window.TcpOptions
                    };
                    svc.SetColorsByType();
                    svc.LogAdded += _viewModel.OnServiceLogAdded;
                    svc.ActiveChanged += _viewModel.OnServiceActiveChanged;
                    GetOrCreateServicePage(svc);
                    _viewModel.Services.Add(svc);
                    _logger?.LogInformation("Service {Name} added", svc.DisplayName);
                    _viewModel.SelectedService = svc;
                    ServiceList.ScrollIntoView(svc);
                }
                else if (window.CreatedServiceType == "FTP Server")
                {
                    var svc = new ServiceViewModel
                    {
                        DisplayName = $"FTP Server - {window.CreatedServiceName}",
                        ServiceType = "FTP Server",
                        IsActive = false,
                        FtpOptions = window.FtpServerOptions
                    };
                    svc.SetColorsByType();
                    svc.LogAdded += _viewModel.OnServiceLogAdded;
                    svc.ActiveChanged += _viewModel.OnServiceActiveChanged;
                    GetOrCreateServicePage(svc);
                    var opt = App.AppHost.Services.GetRequiredService<IOptions<FtpServerOptions>>().Value;
                    if (window.FtpServerOptions != null)
                    {
                        opt.Port = window.FtpServerOptions.Port;
                        opt.RootPath = window.FtpServerOptions.RootPath;
                        opt.AllowAnonymous = window.FtpServerOptions.AllowAnonymous;
                        opt.Username = window.FtpServerOptions.Username;
                        opt.Password = window.FtpServerOptions.Password;
                    }
                    _viewModel.Services.Add(svc);
                    _logger?.LogInformation("Service {Name} added", svc.DisplayName);
                    _viewModel.SelectedService = svc;
                    ServiceList.ScrollIntoView(svc);
                }
                else if (!string.IsNullOrWhiteSpace(window.CreatedServiceType))
                {
                    var svc = new ServiceViewModel
                    {
                        DisplayName = $"{window.CreatedServiceType} - {window.CreatedServiceName}",
                        ServiceType = window.CreatedServiceType,
                        IsActive = false
                    };
                    svc.SetColorsByType();
                    svc.LogAdded += _viewModel.OnServiceLogAdded;
                    svc.ActiveChanged += _viewModel.OnServiceActiveChanged;
                    GetOrCreateServicePage(svc);
                    _viewModel.Services.Add(svc);
                    _logger?.LogInformation("Service {Name} added", svc.DisplayName);
                    _viewModel.SelectedService = svc;
                    ServiceList.ScrollIntoView(svc);
                }
            }
        }

        private void AddMqttService(string name, MqttServiceOptions options)
        {
            var newService = new ServiceViewModel
            {
                DisplayName = $"MQTT - {name}",
                ServiceType = "MQTT",
                IsActive = false
            };

            newService.SetColorsByType();
            newService.LogAdded += _viewModel.OnServiceLogAdded;
            newService.ActiveChanged += _viewModel.OnServiceActiveChanged;

            GetOrCreateServicePage(newService);

            var opt = App.AppHost.Services.GetRequiredService<IOptions<MqttServiceOptions>>().Value;
            opt.Host = options.Host;
            opt.Port = options.Port;
            opt.ClientId = options.ClientId;
            opt.Username = options.Username;
            opt.Password = options.Password;
            opt.UseTls = options.UseTls;
            opt.WillTopic = options.WillTopic;
            opt.WillPayload = options.WillPayload;
            opt.WillQualityOfService = options.WillQualityOfService;
            opt.WillRetain = options.WillRetain;
            opt.KeepAliveSeconds = options.KeepAliveSeconds;
            opt.CleanSession = options.CleanSession;
            opt.ReconnectDelay = options.ReconnectDelay;

            _viewModel.Services.Add(newService);
            _logger?.LogInformation("Service {Name} added", newService.DisplayName);
            _viewModel.SelectedService = newService;
            ServiceList.ScrollIntoView(newService);

            if (newService.ServicePage is MqttTagSubscriptionsView mqttView)
            {
                var mqttVm = (MqttTagSubscriptionsViewModel)mqttView.DataContext!;
                newService.ActiveChanged += async active =>
                {
                    if (active)
                        await mqttVm.ConnectAsync();
                };

                mqttVm.EditConnectionRequested += (_, _) =>
                {
                    var editView = App.AppHost.Services.GetRequiredService<MqttEditConnectionView>();
                    if (editView.DataContext is MqttEditConnectionViewModel vm)
                    {
                        var options = App.AppHost.Services.GetRequiredService<IOptions<MqttServiceOptions>>().Value;
                        vm.Load(options);
                        vm.HighlightMissingFields();
                        vm.RequestClose += (_, _) =>
                        {
                            if (newService.ServicePage != null)
                                ShowPage(newService.ServicePage);
                            _viewModel.SaveServices();
                        };
                    }
                    ShowPage(editView);
                };
            }

            if (newService.ServicePage != null)
            {
                ShowPage(newService.ServicePage);
            }

            _viewModel.SaveServices();
            _logger?.LogDebug("AddService workflow completed");
        }

        private void OnEditRequested(ServiceViewModel service)
        {
            _logger?.LogDebug("Edit requested for {Name}", service.DisplayName);

            if (service.ServiceType == "MQTT")
            {
                var tagPage = GetOrCreateServicePage(service);
                var editView = App.AppHost.Services.GetRequiredService<MqttEditConnectionView>();
                if (editView.DataContext is MqttEditConnectionViewModel vm)
                {
                    var options = App.AppHost.Services.GetRequiredService<IOptions<MqttServiceOptions>>().Value;
                    vm.Load(options);
                    vm.RequestClose += (_, _) =>
                    {
                        if (tagPage != null)
                            ShowPage(tagPage);
                        _viewModel.SaveServices();
                    };
                }
                ShowPage(editView);
                _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
                return;
            }

            if (service.ServiceType == "FTP Server")
            {
                var page = GetOrCreateServicePage(service);
                var options = service.FtpOptions ?? new FtpServerOptions();
                var vm = ActivatorUtilities.CreateInstance<FtpServerEditViewModel>(App.AppHost.Services, service.DisplayName.Split(" - ").Last(), options);
                var editView = App.AppHost.Services.GetRequiredService<FtpServerEditView>();
                editView.DataContext = vm;
                vm.ServerUpdated += (name, opts) =>
                {
                    service.DisplayName = $"FTP Server - {name}";
                    service.FtpOptions = opts;
                    var opt = App.AppHost.Services.GetRequiredService<IOptions<FtpServerOptions>>().Value;
                    opt.Port = opts.Port;
                    opt.RootPath = opts.RootPath;
                    opt.AllowAnonymous = opts.AllowAnonymous;
                    opt.Username = opts.Username;
                    opt.Password = opts.Password;
                    if (page != null)
                        ShowPage(page);
                    _viewModel.SaveServices();
                };
                vm.Cancelled += () =>
                {
                    if (page != null)
                        ShowPage(page);
                };
                vm.AdvancedConfigRequested += opts =>
                {
                    var advVm = ActivatorUtilities.CreateInstance<FtpServerAdvancedConfigViewModel>(App.AppHost.Services, opts);
                    var advView = App.AppHost.Services.GetRequiredService<FtpServerAdvancedConfigView>();
                    advView.DataContext = advVm;
                    advVm.Saved += _ => ShowPage(editView);
                    advVm.Cancelled += () => ShowPage(editView);
                    ShowPage(advView);
                };
                ShowPage(editView);
                _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
                return;
            }

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

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilterPopup != null)
            {
                FilterPopup.IsOpen = !FilterPopup.IsOpen;
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
            if (e.ClickCount == 2)
            {
                HeaderBar_MouseDoubleClick(sender, e);
                e.Handled = true;
                return;
            }

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
                if (_viewModel.EditServiceCommand.CanExecute(svc))
                {
                    _viewModel.EditServiceCommand.Execute(svc);
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
