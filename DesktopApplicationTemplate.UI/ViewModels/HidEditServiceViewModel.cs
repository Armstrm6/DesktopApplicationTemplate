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
    private string _messageTemplate;
    private string _selectedUsbProtocol;
    private string _attachedService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HidEditServiceViewModel"/> class.
    /// </summary>
    public HidEditServiceViewModel(IServiceRule rule, string serviceName, HidServiceOptions options, ILoggingService? logger = null)
        : base(rule, logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _messageTemplate = options.MessageTemplate;
        _selectedUsbProtocol = options.UsbProtocol;
        _attachedService = options.AttachedService;
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
        RaiseServiceSaved(_options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => RaiseEditCancelled();

    /// <inheritdoc />
    protected override void OnAdvancedConfig() => RaiseAdvancedConfigRequested(_options);
}

