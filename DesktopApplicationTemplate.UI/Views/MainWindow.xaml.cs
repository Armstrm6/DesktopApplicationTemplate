using System;
using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Navigation;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Factories;
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
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Converters;

namespace DesktopApplicationTemplate.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [SupportedOSPlatform("windows")]
    public partial class MainView : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly ILogger<MainView>? _logger;
        private readonly IDictionary<ServiceType, Action<ServiceListModel>> _editHandlers;

        public MainView(MainViewModel viewModel, IDictionary<ServiceType, Action<ServiceListModel>> editHandlers)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _editHandlers = editHandlers;
            if (App.AppHost.Services.GetService(typeof(ILoggerFactory)) is ILoggerFactory factory)
            {
                _logger = factory.CreateLogger<MainView>();
            }
            DataContext = _viewModel;
            _viewModel.EditRequested += OnEditRequested;
            _viewModel.AddServiceRequested += OnAddServiceRequested;
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

        public void ShowPage(Page page)
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


        public Page? GetOrCreateServicePage(ServiceListModel svc)
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
                "FTP Server" or "FTP" => App.AppHost.Services.GetRequiredService<FTPServiceView>(),
                "CSV Creator" => App.AppHost.Services.GetRequiredService<CsvServiceView>(),
                _ => null
            };

            if (svc.ServicePage != null)
            {
                if (svc.ServicePage.DataContext is ILoggingViewModel vm && vm.Logger is LoggingService logger)
                {
                    if (svc.ServiceType == "MQTT")
                    {
                        logger.LogAdded += entry => _viewModel.OnServiceLogAdded(svc, entry);
                    }
                    else
                    {
                        logger.LogAdded += entry =>
                        {
                            var brush = (Brush?)new BrushConverter().ConvertFromString(entry.Color);
                            svc.AddLog(entry.Message, brush, entry.Level);
                        };
                    }
                }

                if (svc.ServicePage.DataContext is INetworkAwareViewModel navm)
                {
                    navm.UpdateNetworkConfiguration(_viewModel.NetworkConfig.CurrentConfiguration);
                }

                if (svc.ServicePage is IServiceLogHost logHost)
                {
                    logHost.SetServiceContext(svc);
                }

                if (svc.ServiceType == "TCP" && svc.ServicePage.DataContext is TcpServiceMessagesViewModel tcpVm)
                {
                    tcpVm.AdvancedSettingsRequested += (_, _) =>
                    {
                        var vm = App.AppHost.Services.GetRequiredService<TcpEditServiceViewModel>();
                        vm.ServiceType = svc.ServiceType;
                        vm.Load(svc.DisplayName.Split(" - ").Last(), svc.TcpOptions ?? new TcpServiceOptions());
                        var editView = App.AppHost.Services.GetRequiredService<TcpEditServiceView>();
                        editView.Initialize(vm);

                        vm.ServiceSaved += (name, opts) =>
                        {
                            svc.DisplayName = $"{vm.ServiceType} - {name}";
                            svc.ServiceType = vm.ServiceType;
                            svc.TcpOptions = opts;
                            if (svc.ServicePage != null)
                                ShowPage(svc.ServicePage);
                            _ = _viewModel.SaveServicesAsync();
                        };
                        vm.EditCancelled += () =>
                        {
                            if (svc.ServicePage != null)
                                ShowPage(svc.ServicePage);
                        };

                        ShowPage(editView);
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
            ShowCreateServiceSelectionPage();
        }

        public void ShowCreateServiceSelectionPage()
        {
            var page = App.AppHost.Services.GetRequiredService<CreateServicePage>();
            _createServicePage = page;
            page.ServiceCreated += (name, type) =>
            {
                var svc = new ServiceListModel
                {
                    DisplayName = $"{type} - {name}",
                    ServiceType = type,
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
                if (svc.ServicePage != null)
                    ShowPage(svc.ServicePage);
                _ = _viewModel.SaveServicesAsync();
            };
            page.ServiceTypeSelected += NavigateTo;
            page.Cancelled += ShowHome;
            ShowPage(page);
        }

        private CreateServicePage? _createServicePage;

        private void NavigateTo(string serviceType)
        {
            var defaultName = _createServicePage?.GenerateDefaultName(serviceType) ?? serviceType;
            var handler = App.AppHost.Services.GetServices<INavigationHandler>()
                .FirstOrDefault(h => h.ServiceType == serviceType);
            if (handler == null)
                return;
            var view = handler.CreateView(defaultName);
            ShowPage(view);
        }









        internal async Task AddServiceAsync(string type, object options)
        {
            var factory = App.AppHost.Services.GetServices<IServiceFactory>()
                .FirstOrDefault(f => f.ServiceType == type);
            if (factory == null)
                return;

            var svc = factory.Create(options);
            svc.SetColorsByType();
            svc.LogAdded += _viewModel.OnServiceLogAdded;
            svc.ActiveChanged += _viewModel.OnServiceActiveChanged;

            _viewModel.Services.Add(svc);
            _logger?.LogInformation("Service {Name} added", svc.DisplayName);
            _viewModel.SelectedService = svc;
            ServiceList.ScrollIntoView(svc);
            if (svc.ServicePage != null)
                ShowPage(svc.ServicePage);
            await _viewModel.SaveServicesAsync();
            _logger?.LogDebug("AddService workflow completed");
        }

    private void OnEditRequested(ServiceListModel service)
    {
        _logger?.LogDebug("Edit requested for {Name}", service.DisplayName);

        if (ServiceTypeJsonConverter.TryParse(service.ServiceType, out var type) &&
            _editHandlers.TryGetValue(type, out var handler))
        {
            handler(service);
            return;
        }

        var servicePage = GetOrCreateServicePage(service);
        if (servicePage != null)
        {
            ShowPage(servicePage);
            _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
        }
    }

    internal void EditMqtt(ServiceListModel service)
    {
        var tagPage = GetOrCreateServicePage(service);
        var options = App.AppHost.Services.GetRequiredService<IOptions<MqttServiceOptions>>().Value;
        var vm = ActivatorUtilities.CreateInstance<MqttEditServiceViewModel>(App.AppHost.Services, service.DisplayName.Split(" - ").Last(), options);
        var editView = App.AppHost.Services.GetRequiredService<MqttEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"MQTT - {name}";
            if (tagPage != null)
                ShowPage(tagPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (tagPage != null)
                ShowPage(tagPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<MqttAdvancedConfigViewModel>(App.AppHost.Services, opts);
            var advView = App.AppHost.Services.GetRequiredService<MqttAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => ShowPage(editView);
            advVm.BackRequested += () => ShowPage(editView);
            ShowPage(advView);
        };
        ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }

    internal void EditHeartbeat(ServiceListModel service)
    {
        var hbPage = GetOrCreateServicePage(service);
        var options = service.HeartbeatOptions ?? new HeartbeatServiceOptions();
        var vm = ActivatorUtilities.CreateInstance<HeartbeatEditServiceViewModel>(App.AppHost.Services, service.DisplayName.Split(" - ").Last(), options);
        var editView = App.AppHost.Services.GetRequiredService<HeartbeatEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"Heartbeat - {name}";
            service.HeartbeatOptions = opts;
            if (hbPage != null)
                ShowPage(hbPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (hbPage != null)
                ShowPage(hbPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<HeartbeatAdvancedConfigViewModel>(App.AppHost.Services, opts);
            var advView = App.AppHost.Services.GetRequiredService<HeartbeatAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => ShowPage(editView);
            advVm.BackRequested += () => ShowPage(editView);
            ShowPage(advView);
        };
        ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }

    internal void EditHid(ServiceListModel service)
    {
        var hidPage = GetOrCreateServicePage(service);
        var options = service.HidOptions ?? new HidServiceOptions();
        var vm = ActivatorUtilities.CreateInstance<HidEditServiceViewModel>(App.AppHost.Services, service.DisplayName.Split(" - ").Last(), options);
        var editView = App.AppHost.Services.GetRequiredService<HidEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"HID - {name}";
            service.HidOptions = opts;
            if (hidPage != null)
                ShowPage(hidPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (hidPage != null)
                ShowPage(hidPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<HidAdvancedConfigViewModel>(App.AppHost.Services, opts);
            var advView = App.AppHost.Services.GetRequiredService<HidAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => ShowPage(editView);
            advVm.BackRequested += () => ShowPage(editView);
            ShowPage(advView);
        };
        ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }

    internal void EditCsv(ServiceListModel service)
    {
        var csvPage = GetOrCreateServicePage(service);
        var options = service.CsvOptions ?? new CsvServiceOptions();
        var vm = App.AppHost.Services.GetRequiredService<CsvServiceEditorViewModel>();
        vm.Load(service.DisplayName.Split(" - ").Last(), options);
        var editView = App.AppHost.Services.GetRequiredService<CsvServiceEditorView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"CSV Creator - {name}";
            service.CsvOptions = opts;
            if (csvPage != null)
                ShowPage(csvPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (csvPage != null)
                ShowPage(csvPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<CsvAdvancedConfigViewModel>(App.AppHost.Services, opts);
            var advView = App.AppHost.Services.GetRequiredService<CsvAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => ShowPage(editView);
            advVm.BackRequested += () => ShowPage(editView);
            ShowPage(advView);
        };
        ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }

    internal void EditFileObserver(ServiceListModel service)
    {
        var foPage = GetOrCreateServicePage(service);
        var options = service.FileObserverOptions ?? new FileObserverServiceOptions();
        var vm = ActivatorUtilities.CreateInstance<FileObserverEditServiceViewModel>(App.AppHost.Services, service.DisplayName.Split(" - ").Last(), options);
        var editView = App.AppHost.Services.GetRequiredService<FileObserverEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"File Observer - {name}";
            service.FileObserverOptions = opts;
            if (foPage != null)
                ShowPage(foPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (foPage != null)
                ShowPage(foPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<FileObserverAdvancedConfigViewModel>(App.AppHost.Services, opts);
            var advView = App.AppHost.Services.GetRequiredService<FileObserverAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => ShowPage(editView);
            advVm.BackRequested += () => ShowPage(editView);
            ShowPage(advView);
        };
        ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }

    internal void EditScp(ServiceListModel service)
    {
        var scpPage = GetOrCreateServicePage(service);
        var options = service.ScpOptions ?? new ScpServiceOptions();
        var vm = App.AppHost.Services.GetRequiredService<ScpEditServiceViewModel>();
        vm.Load(service.DisplayName.Split(" - ").Last(), options);
        var editView = App.AppHost.Services.GetRequiredService<ScpEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"SCP - {name}";
            service.ScpOptions = opts;
            if (scpPage != null)
                ShowPage(scpPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (scpPage != null)
                ShowPage(scpPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = App.AppHost.Services.GetRequiredService<ScpAdvancedConfigViewModel>();
            advVm.Load(opts);
            var advView = App.AppHost.Services.GetRequiredService<ScpAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => ShowPage(editView);
            advVm.BackRequested += () => ShowPage(editView);
            ShowPage(advView);
        };
        ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }

    internal void EditTcp(ServiceListModel service)
    {
        var tcpPage = GetOrCreateServicePage(service);
        var options = service.TcpOptions ?? new TcpServiceOptions();
        var vm = App.AppHost.Services.GetRequiredService<TcpEditServiceViewModel>();
        vm.ServiceType = service.ServiceType;
        vm.Load(service.DisplayName.Split(" - ").Last(), options);
        var editView = App.AppHost.Services.GetRequiredService<TcpEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"{vm.ServiceType} - {name}";
            service.ServiceType = vm.ServiceType;
            service.TcpOptions = opts;
            if (tcpPage != null)
                ShowPage(tcpPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (tcpPage != null)
                ShowPage(tcpPage);
        };
        ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }

    internal void EditHttp(ServiceListModel service)
    {
        var httpPage = GetOrCreateServicePage(service);
        var options = service.HttpOptions ?? new HttpServiceOptions();
        var vm = ActivatorUtilities.CreateInstance<HttpEditServiceViewModel>(App.AppHost.Services, service.DisplayName.Split(" - ").Last(), options);
        var editView = App.AppHost.Services.GetRequiredService<HttpEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"HTTP - {name}";
            service.HttpOptions = opts;
            if (httpPage != null)
                ShowPage(httpPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (httpPage != null)
                ShowPage(httpPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<HttpAdvancedConfigViewModel>(App.AppHost.Services, opts);
            var advView = App.AppHost.Services.GetRequiredService<HttpAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => ShowPage(editView);
            advVm.BackRequested += () => ShowPage(editView);
            ShowPage(advView);
        };
        ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }

    internal void EditFtp(ServiceListModel service)
    {
        var ftpPage = GetOrCreateServicePage(service);
        var options = service.FtpOptions ?? new FtpServerOptions();
        var vm = ActivatorUtilities.CreateInstance<FtpServerEditViewModel>(App.AppHost.Services, service.DisplayName.Split(" - ").Last(), options);
        var editView = ActivatorUtilities.CreateInstance<FtpServerEditView>(App.AppHost.Services, vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"FTP Server - {name}";
            service.FtpOptions = opts;
            var opt = App.AppHost.Services.GetRequiredService<IOptions<FtpServerOptions>>().Value;
            opt.Port = opts.Port;
            opt.RootPath = opts.RootPath;
            opt.AllowAnonymous = opts.AllowAnonymous;
            opt.Username = opts.Username;
            opt.Password = opts.Password;
            if (ftpPage != null)
                ShowPage(ftpPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (ftpPage != null)
                ShowPage(ftpPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<FtpServerAdvancedConfigViewModel>(App.AppHost.Services, opts);
            var advView = App.AppHost.Services.GetRequiredService<FtpServerAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => ShowPage(editView);
            advVm.BackRequested += () => ShowPage(editView);
            ShowPage(advView);
        };
        ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
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

        private void DeleteServiceMenu_Click(object sender, RoutedEventArgs e) => _ = DeleteServiceMenuAsync(sender, e);

        private async Task DeleteServiceMenuAsync(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem { DataContext: ServiceListModel svc })
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
                await _viewModel.SaveServicesAsync();
            }
        }

        private void RenameServiceMenu_Click(object sender, RoutedEventArgs e) => _ = RenameServiceMenuAsync(sender, e);

        private async Task RenameServiceMenuAsync(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem { DataContext: ServiceListModel svc })
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
                    await _viewModel.SaveServicesAsync();
                }
            }
        }

        private void ChangeColorMenu_Click(object sender, RoutedEventArgs e) => _ = ChangeColorMenuAsync(sender, e);

        private async Task ChangeColorMenuAsync(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem { DataContext: ServiceListModel svc })
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
                    await _viewModel.SaveServicesAsync();
                }
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

            if (e.OriginalSource is not DependencyObject element ||
                Helpers.VisualTreeHelperExtensions.FindParent<ButtonBase>(element) == null)
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
            if (e.OriginalSource is DependencyObject element &&
                Helpers.VisualTreeHelperExtensions.FindParent<ButtonBase>(element) != null)
                return;

            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

            _logger?.LogInformation("Window state changed to {State}", WindowState);
            e.Handled = true;
        }

        private void MainView_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is not DependencyObject element)
                return;

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

            if (sender is Border { DataContext: ServiceListModel svc })
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
                if (sender is Border { DataContext: ServiceListModel svc } border)
                {
                    DragDrop.DoDragDrop(border, svc, System.Windows.DragDropEffects.Move);
                }
            }
        }

        private void ServiceItem_Drop(object sender, System.Windows.DragEventArgs e) => _ = ServiceItemDropAsync(sender, e);

        private async Task ServiceItemDropAsync(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(ServiceListModel)))
                return;

            var source = (ServiceListModel)e.Data.GetData(typeof(ServiceListModel))!;
            if (sender is not Border { DataContext: ServiceListModel target } || source == target)
                return;

            int oldIndex = _viewModel.Services.IndexOf(source);
            int newIndex = _viewModel.Services.IndexOf(target);
            if (oldIndex != newIndex)
            {
                _viewModel.Services.Move(oldIndex, newIndex);
                await _viewModel.SaveServicesAsync();
            }
        }

        // Legacy handler removed; log level selection is now managed by ServiceLogView.

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
