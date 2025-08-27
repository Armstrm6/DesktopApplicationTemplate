using System;
using System.Collections.Generic;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for creating a new HID service.
/// </summary>
public class HidCreateServiceViewModel : ViewModelBase, ILoggingViewModel
{
    private string _serviceName = string.Empty;
    private string _messageTemplate = string.Empty;
    private string _selectedUsbProtocol = "2.0";
    private string _attachedService = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="HidCreateServiceViewModel"/> class.
    /// </summary>
    public HidCreateServiceViewModel(ILoggingService? logger = null)
    {
        Logger = logger;
        CreateCommand = new RelayCommand(Create);
        CancelCommand = new RelayCommand(Cancel);
        AdvancedConfigCommand = new RelayCommand(OpenAdvancedConfig);
        UsbProtocols = new[] { "2.0", "3.0" };
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, HidServiceOptions>? ServiceCreated;

    /// <summary>
    /// Raised when creation is cancelled.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<HidServiceOptions>? AdvancedConfigRequested;

    /// <summary>
    /// Command to create the service.
    /// </summary>
    public ICommand CreateCommand { get; }

    /// <summary>
    /// Command to cancel creation.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Command to open advanced configuration.
    /// </summary>
    public ICommand AdvancedConfigCommand { get; }

    /// <summary>
    /// Available USB protocol options.
    /// </summary>
    public IReadOnlyList<string> UsbProtocols { get; }

    /// <summary>
    /// Name of the service.
    /// </summary>
    public string ServiceName
    {
        get => _serviceName;
        set { _serviceName = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Message template for the HID service.
    /// </summary>
    public string MessageTemplate
    {
        get => _messageTemplate;
        set { _messageTemplate = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Selected USB protocol.
    /// </summary>
    public string SelectedUsbProtocol
    {
        get => _selectedUsbProtocol;
        set { _selectedUsbProtocol = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Name of the service to forward messages to.
    /// </summary>
    public string AttachedService
    {
        get => _attachedService;
        set { _attachedService = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Current configuration options.
    /// </summary>
    public HidServiceOptions Options { get; } = new();

    private void Create()
    {
        Logger?.Log("HID create options start", LogLevel.Debug);
        Options.MessageTemplate = MessageTemplate;
        Options.UsbProtocol = SelectedUsbProtocol;
        Options.AttachedService = AttachedService;
        Logger?.Log("HID create options finished", LogLevel.Debug);
        ServiceCreated?.Invoke(ServiceName, Options);
    }

    private void Cancel()
    {
        Logger?.Log("HID create cancelled", LogLevel.Debug);
        Cancelled?.Invoke();
    }

    private void OpenAdvancedConfig()
    {
        Logger?.Log("Opening HID advanced config", LogLevel.Debug);
        Options.MessageTemplate = MessageTemplate;
        Options.UsbProtocol = SelectedUsbProtocol;
        Options.AttachedService = AttachedService;
        AdvancedConfigRequested?.Invoke(Options);
    }
}
