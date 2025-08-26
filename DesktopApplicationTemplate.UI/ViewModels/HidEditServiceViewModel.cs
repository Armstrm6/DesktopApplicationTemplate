using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing HID service configuration.
/// </summary>
public class HidEditServiceViewModel : HidCreateServiceViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HidEditServiceViewModel"/> class.
    /// </summary>
    public HidEditServiceViewModel(string serviceName, HidServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        ServiceName = serviceName;
        MessageTemplate = options.MessageTemplate;
        SelectedUsbProtocol = options.UsbProtocol;
        AttachedService = options.AttachedService;
        Options.MessageTemplate = options.MessageTemplate;
        Options.UsbProtocol = options.UsbProtocol;
        Options.AttachedService = options.AttachedService;
        Options.DebounceTimeMs = options.DebounceTimeMs;
        Options.KeyDownTimeMs = options.KeyDownTimeMs;
    }

    /// <summary>
    /// Command for saving the updated configuration.
    /// </summary>
    public ICommand SaveCommand => CreateCommand;

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, HidServiceOptions>? ServiceUpdated
    {
        add => ServiceCreated += value;
        remove => ServiceCreated -= value;
    }
}
