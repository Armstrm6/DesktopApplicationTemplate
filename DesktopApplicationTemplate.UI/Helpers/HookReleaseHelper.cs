using System;
using DesktopApplicationTemplate.UI.Services;

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
        public static void Release()
        {
            try
            {
                KeyboardSimulator.Reset();
            }
            finally
            {
                ReleaseAction?.Invoke();
            }
        }
    }
}
