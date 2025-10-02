using System;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Provides helpers for resetting any simulated keyboard state.
    /// </summary>
    public static class KeyboardSimulator
    {
        /// <summary>
        /// Optional hook for tests to observe resets.
        /// </summary>
        public static Action? ResetAction { get; set; }

        /// <summary>
        /// Resets any simulated keyboard state.
        /// </summary>
        public static void Reset()
        {
            ResetAction?.Invoke();
        }
    }
}
