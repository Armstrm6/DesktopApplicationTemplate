using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace DesktopApplicationTemplate.UI.Helpers
{
    /// <summary>
    /// Provides utilities for interacting with the keyboard.
    /// </summary>
    public static class KeyboardHelper
    {
        private const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        /// <summary>
        /// Releases the specified keys to prevent them from remaining in a pressed state.
        /// </summary>
        /// <param name="keys">Keys to release.</param>
        public static void ReleaseKeys(params Key[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            foreach (var key in keys)
            {
                var virtualKey = (byte)KeyInterop.VirtualKeyFromKey(key);
                keybd_event(virtualKey, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
        }
    }
}
