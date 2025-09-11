using System;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI;

/// <summary>
/// Adapter that invokes an <see cref="Action{T}"/> when editing a service.
/// </summary>
public class DelegateEditServiceHandler : IEditServiceHandler
{
    private readonly Action<ServiceListModel> _action;

    public DelegateEditServiceHandler(Action<ServiceListModel> action)
    {
        _action = action;
    }

    /// <inheritdoc />
    public void Edit(ServiceListModel service) => _action(service);
}
