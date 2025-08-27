using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing HID service configuration.
/// </summary>
public class HidEditServiceViewModel : ServiceEditViewModelBase<HidServiceOptions>
{
    private readonly HidServiceOptions _options;
    private string _serviceName;
    private string _messageTemplate;
    private string _selectedUsbProtocol;
    private string _attachedService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HidEditServiceViewModel"/> class.
    /// </summary>
    public HidEditServiceViewModel(string serviceName, HidServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _messageTemplate = options.MessageTemplate;
        _selectedUsbProtocol = options.UsbProtocol;
        _attachedService = options.AttachedService;
    }

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, HidServiceOptions>? ServiceUpdated;

    /// <summary>
    /// Raised when editing is cancelled.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<HidServiceOptions>? AdvancedConfigRequested;

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

    /// <inheritdoc />
    protected override void OnSave()
    {
        _options.MessageTemplate = MessageTemplate;
        _options.UsbProtocol = SelectedUsbProtocol;
        _options.AttachedService = AttachedService;
        ServiceUpdated?.Invoke(ServiceName, _options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => Cancelled?.Invoke();

    /// <inheritdoc />
    protected override void OnAdvancedConfig() => AdvancedConfigRequested?.Invoke(_options);
}

