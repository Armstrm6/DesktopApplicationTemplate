using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows.Input;

namespace DesktopApplicationTemplate.UI.Helpers
{
    /// <summary>
    /// Provides utilities for interacting with the keyboard.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class KeyboardHelper
    {
        private const int INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private static readonly HashSet<Key> _pressedKeys = new();

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        /// <summary>
        /// Programmatically presses the specified keys.
        /// </summary>
        /// <param name="keys">Keys to press.</param>
        public static void PressKey(params Key[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            foreach (var key in keys)
            {
                lock (_pressedKeys)
                {
                    if (_pressedKeys.Add(key))
                    {
                        Send(key, false);
                    }
                }
            }
        }

        /// <summary>
        /// Releases the specified keys that were programmatically pressed.
        /// </summary>
        /// <param name="keys">Keys to release.</param>
        public static void ReleaseKey(params Key[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            foreach (var key in keys)
            {
                lock (_pressedKeys)
                {
                    if (_pressedKeys.Remove(key))
                    {
                        Send(key, true);
                    }
                }
            }
        }

        internal static bool IsPressed(Key key)
        {
            lock (_pressedKeys)
            {
                return _pressedKeys.Contains(key);
            }
        }

        private static void Send(Key key, bool keyUp)
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            var input = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)KeyInterop.VirtualKeyFromKey(key),
                        wScan = 0,
                        dwFlags = keyUp ? KEYEVENTF_KEYUP : 0,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };

            SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        }
    }
}
