using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Models;
using LogLevel = DesktopApplicationTemplate.Core.Services.LogLevel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Runtime.Versioning;
using System.Threading.Tasks;

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
        private readonly IServiceUiRegistry<ServiceListModel, Page> _serviceRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<ServiceListModel, Action<LogEntry>> _serviceLogHandlers = new();
        private readonly BrushConverter _brushConverter = new();

        public MainView(
            MainViewModel viewModel,
            IServiceUiRegistry<ServiceListModel, Page> serviceRegistry,
            IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _serviceRegistry = serviceRegistry;
            _serviceProvider = serviceProvider;
            if (_serviceProvider.GetService(typeof(ILoggerFactory)) is ILoggerFactory factory)
            {
                _logger = factory.CreateLogger<MainView>();
            }
            DataContext = _viewModel;
            _viewModel.AddServiceRequested += OnAddServiceRequested;
            _viewModel.Services.CollectionChanged += Services_CollectionChanged;
            MouseDown += MainView_MouseDown;
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseCommand_Executed));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeCommand_Executed));
            Closing += (_, _) => _logger?.LogInformation("MainView closing");
            ShowHome();
            PreloadServicePages();
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

        private void HomeButton_Click(object sender, RoutedEventArgs e) => NavigateHome("Home button");

        private void AppTitle_Click(object sender, RoutedEventArgs e)
        {
            NavigateHome("Application title");
            e.Handled = true;
        }

        private void NavigateHome(string source)
        {
            _logger?.LogInformation("{Source} clicked", source);
            _viewModel.SelectedService = null;
            ShowHome();
        }


        public Page? GetOrCreateServicePage(ServiceListModel svc)
        {
            if (svc.ServicePage == null && _serviceRegistry.TryCreateServicePage(svc.Type, _serviceProvider, out var page))
            {
                svc.ServicePage = page;
            }

            if (svc.ServicePage != null)
            {
                if (svc.ServicePage.DataContext is ILoggingViewModel vm)
                {
                    AttachServiceLogger(svc, vm);
                }

                if (svc.ServicePage.DataContext is INetworkAwareViewModel navm)
                {
                    navm.UpdateNetworkConfiguration(_viewModel.NetworkConfig.CurrentConfiguration);
                }

                if (svc.ServicePage is IServiceLogHost logHost)
                {
                    logHost.SetServiceContext(svc);
                }
            }

            return svc.ServicePage;
        }

        private void AttachServiceLogger(ServiceListModel svc, ILoggingViewModel vm)
        {
            if (vm.Logger is null || _serviceLogHandlers.ContainsKey(svc))
            {
                return;
            }

            Action<LogEntry> handler;

            if (svc.Type == ServiceType.Mqtt)
            {
                handler = entry => _viewModel.OnServiceLogAdded(svc, entry);
            }
            else
            {
                handler = entry =>
                {
                    Brush? brush = null;
                    try
                    {
                        brush = _brushConverter.ConvertFromString(entry.Color) as Brush;
                    }
                    catch
                    {
                        brush = null;
                    }

                    svc.AddLog(entry.Message, brush ?? Brushes.Black, entry.Level);
                };
            }

            vm.Logger.LogAdded += handler;
            _serviceLogHandlers[svc] = handler;

            if (svc.Logs.Count == 0)
            {
                vm.Logger.Reload();
            }
        }

        private void DetachServiceLogger(ServiceListModel svc)
        {
            if (!_serviceLogHandlers.TryGetValue(svc, out var handler))
            {
                return;
            }

            if (svc.ServicePage?.DataContext is ILoggingViewModel vm && vm.Logger is not null)
            {
                vm.Logger.LogAdded -= handler;
            }

            _serviceLogHandlers.Remove(svc);
        }

        private void Services_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace)
            {
                foreach (ServiceListModel svc in e.OldItems?.OfType<ServiceListModel>() ?? Enumerable.Empty<ServiceListModel>())
                {
                    DetachServiceLogger(svc);
                }
            }

            if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace)
            {
                foreach (ServiceListModel svc in e.NewItems?.OfType<ServiceListModel>() ?? Enumerable.Empty<ServiceListModel>())
                {
                    GetOrCreateServicePage(svc);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var svc in _serviceLogHandlers.Keys.ToList())
                {
                    DetachServiceLogger(svc);
                }
            }
        }

        private void PreloadServicePages()
        {
            foreach (var svc in _viewModel.Services)
            {
                GetOrCreateServicePage(svc);
            }
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
            var page = _serviceProvider.GetRequiredService<CreateServicePage>();
            _createServicePage = page;
            page.ServiceCreated += (name, type) =>
            {
                var svc = new ServiceListModel
                {
                    DisplayName = $"{type.ToLegacyString()} - {name}",
                    Type = type,
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

        private void NavigateTo(ServiceType serviceType)
        {
            var defaultName = _createServicePage?.GenerateDefaultName(serviceType) ?? serviceType.ToLegacyString();
            if (_serviceRegistry.TryCreateNavigationPage(serviceType, _serviceProvider, defaultName, out var view) &&
                view is not null)
            {
                ShowPage(view);
            }
        }









        internal async Task AddServiceAsync(ServiceType type, object options)
        {
            if (!_serviceRegistry.TryCreateService(type, _serviceProvider, options, out var svc) || svc is null)
            {
                return;
            }
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
            await _viewModel.SaveServicesAsync();
            _logger?.LogDebug("AddService workflow completed");
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
                string input = Microsoft.VisualBasic.Interaction.InputBox("Enter new service name:", "Rename Service", svc.DisplayName);
                if (!string.IsNullOrWhiteSpace(input))
                {
                    var namePart = input.Contains(" - ") ? input.Split(" - ").Last() : input;
                    if (_viewModel.Services.Any(s => s != svc && s.DisplayName.Split(" - ").Last().Equals(namePart, StringComparison.OrdinalIgnoreCase)))
                    {
                        namePart = _viewModel.GenerateServiceName(svc.Type);
                    }
                    svc.DisplayName = $"{svc.Type.ToLegacyString()} - {namePart}";
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
                    foreach (var s in _viewModel.Services.Where(s => s.Type == svc.Type))
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
            var page = _serviceProvider.GetRequiredService<SettingsPage>();
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
