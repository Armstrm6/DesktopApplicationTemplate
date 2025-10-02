using System;
using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels.Hid.Create;

/// <summary>
/// View model for creating a new HID service.
/// </summary>
public class HidCreateServiceViewModel : ServiceCreateViewModelBase<HidServiceOptions>
{
    private string _messageTemplate = string.Empty;
    private string _selectedUsbProtocol = "2.0";
    private string _attachedService = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="HidCreateServiceViewModel"/> class.
    /// </summary>
    public HidCreateServiceViewModel(IServiceRule rule, ILoggingService? logger = null)
        : base(rule, logger: logger)
    {
        UsbProtocols = new[] { "2.0", "3.0" };
    }

    /// <summary>
    /// Available USB protocol options.
    /// </summary>
    public IReadOnlyList<string> UsbProtocols { get; }

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
    protected override void ApplyOptions(HidServiceOptions options)
    {
        options.MessageTemplate = MessageTemplate;
        options.UsbProtocol = SelectedUsbProtocol;
        options.AttachedService = AttachedService;
    }
}
