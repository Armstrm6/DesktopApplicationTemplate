using System;
using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using FormsColor = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using WpfColor = System.Windows.Media.Color;
using System.Windows.Media;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Views;

namespace DesktopApplicationTemplate.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private readonly MainViewModel _viewModel;

        public MainView(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            _viewModel.EditRequested += OnEditRequested;
            ContentFrame.Content = new HomePage { DataContext = _viewModel };
            KeyDown += MainView_KeyDown;
            MouseDown += MainView_MouseDown;
        }

        private Page? GetOrCreateServicePage(ServiceViewModel svc)
        {
            if (svc.ServicePage != null)
                return svc.ServicePage;

            svc.ServicePage = svc.ServiceType switch
            {
                "TCP" => new TcpServiceView(
                    App.AppHost.Services.GetRequiredService<TcpServiceViewModel>(),
                    App.AppHost.Services.GetRequiredService<IStartupService>()),
                "HTTP" => App.AppHost.Services.GetRequiredService<HttpServiceView>(),
                "File Observer" => App.AppHost.Services.GetRequiredService<FileObserverView>(),
                "HID" => new HidViews(),
                "Heartbeat" => new HeartbeatView(App.AppHost.Services.GetRequiredService<HeartbeatViewModel>()),
                "SCP" => new SCPServiceView(App.AppHost.Services.GetRequiredService<ScpServiceViewModel>()),
                "MQTT" => new MQTTServiceView(App.AppHost.Services.GetRequiredService<MqttServiceViewModel>()),
                "FTP" => new FTPServiceView(App.AppHost.Services.GetRequiredService<FtpServiceViewModel>()),
                _ => null
            };

            if (svc.ServicePage != null)
            {
                if (svc.ServicePage.DataContext is ILoggingViewModel vm && vm.Logger is LoggingService logger)
                {
                    if (vm.Logger != null)
                    {
                        logger.LogAdded += entry => svc.AddLog(entry.Message, entry.Color);
                    }
                }
            }

            return svc.ServicePage;
        }

        private void AddService_Click(object sender, RoutedEventArgs e)
        {
            // Open the service creation workflow in its own window
            var window = App.AppHost.Services.GetRequiredService<CreateServiceWindow>();

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
                _viewModel.SelectedService = newService;

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
                    var editor = new ServiceEditorWindow(newService.ServicePage);
                    editor.ShowDialog();
                    ContentFrame.Content = new HomePage { DataContext = _viewModel };
                }
                _viewModel.SaveServices();
            }
        }

        private void OnEditRequested(ServiceViewModel service)
        {
            var page = GetOrCreateServicePage(service);
            if (page != null)
            {
                var editor = new ServiceEditorWindow(page);
                editor.ShowDialog();
                ContentFrame.Content = new HomePage { DataContext = _viewModel };
            }
        }

        private void RemoveService_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm &&
                vm.RemoveServiceCommand.CanExecute(null))
            {
                vm.RemoveServiceCommand.Execute(null);
            }
        }
        private void EditService_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedService == null)
                return;

            if (_viewModel.SelectedService.ServiceType == "CSV Creator")
            {
                var vm = App.AppHost.Services.GetRequiredService<CsvViewerViewModel>();
                var window = new CsvViewerWindow(vm);
                vm.RequestClose += () => window.Close();
                window.ShowDialog();
            }
            else
            {
                var page = GetOrCreateServicePage(_viewModel.SelectedService);
                if (page != null)
                {
                    _viewModel.SelectedService.IsActive = false;
                    var editor = new ServiceEditorWindow(page);
                    editor.ShowDialog();
                    ContentFrame.Content = new HomePage { DataContext = _viewModel };
                }
            }
        }

        private void ServiceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ContentFrame.Content = new HomePage { DataContext = _viewModel };
        }

        private void ServiceItem_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Ensure the handler only executes on a true double click
            if (e.ClickCount != 2)
                return;

            if ((sender as Border)?.DataContext is ServiceViewModel svc)
            {
                if (svc.ServiceType == "CSV Creator")
                {
                    var vm = App.AppHost.Services.GetRequiredService<CsvViewerViewModel>();
                    var window = new CsvViewerWindow(vm);
                    vm.RequestClose += () => window.Close();
                    window.ShowDialog();
                    return;
                }

                var page = GetOrCreateServicePage(svc);
                if (page != null)
                {
                    svc.IsActive = false;
                    var editor = new ServiceEditorWindow(page);
                    editor.ShowDialog();
                    ContentFrame.Content = new HomePage { DataContext = _viewModel };
                }
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
                if (svc.ServiceType == "CSV Creator")
                {
                    var vm = App.AppHost.Services.GetRequiredService<CsvViewerViewModel>();
                    var window = new CsvViewerWindow(vm);
                    vm.RequestClose += () => window.Close();
                    window.ShowDialog();
                    return;
                }

                var page = GetOrCreateServicePage(svc);
                if (page != null)
                {
                    svc.IsActive = false;
                    var editor = new ServiceEditorWindow(page);
                    editor.ShowDialog();
                    ContentFrame.Content = new HomePage { DataContext = _viewModel };
                }
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
                    svc.DisplayName = input;
                    _viewModel.SaveServices();
                }
            }
        }

        private void ChangeColorMenu_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is ServiceViewModel svc)
            {
                var dlg = new System.Windows.Forms.ColorDialog();
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var color = MediaColor.FromArgb(dlg.Color.A, dlg.Color.R, dlg.Color.G, dlg.Color.B);
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
                ContentFrame.Content = new HomePage { DataContext = _viewModel };
            }
        }

        private void MainView_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as DependencyObject;
            if (_viewModel.SelectedService != null &&
                Helpers.VisualTreeHelperExtensions.FindParent<ListBoxItem>(element) == null &&
                Helpers.VisualTreeHelperExtensions.FindParent<Frame>(element) == null)
            {
                _viewModel.SelectedService = null;
                ContentFrame.Content = new HomePage { DataContext = _viewModel };
            }
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            var page = new SettingsPage(App.AppHost.Services.GetRequiredService<SettingsViewModel>());
            ContentFrame.Content = page;
        }

    }
}
