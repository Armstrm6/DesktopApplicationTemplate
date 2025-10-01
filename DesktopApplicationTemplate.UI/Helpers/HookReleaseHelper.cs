using System;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.Helpers
{
    /// <summary>
    /// Provides methods to release input hooks such as HID devices or keyboard simulators.
    /// </summary>
    public static class HookReleaseHelper
    {
        /// <summary>
        /// Optional callback for tests to observe hook release.
        /// </summary>
        public static Action? ReleaseAction { get; set; }

        /// <summary>
        /// Releases any installed HID or keyboard hooks.
        /// </summary>
        public static void Release(IServiceProvider? serviceProvider = null)
        {
            try
            {
                serviceProvider ??= App.AppHost?.Services;
                if (serviceProvider is not null)
                {
                    var releaseServices = serviceProvider.GetServices<IHookReleaseService>();
                    foreach (var releaseService in releaseServices)
                    {
                        releaseService?.Release();
                    }
                }
            }
            finally
            {
                ReleaseAction?.Invoke();
            }
        }
    }
}
