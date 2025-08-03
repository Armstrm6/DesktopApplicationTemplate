using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    internal class KeyboardHidDevice : IHidDevice
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_UNICODE = 0x0004;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        public async Task SendKeystrokesAsync(string text, int keyDownTimeMs, int debounceTimeMs, CancellationToken token = default)
        {
            foreach (var ch in text)
            {
                token.ThrowIfCancellationRequested();
                var inputs = new[]
                {
                    new INPUT { type = INPUT_KEYBOARD, u = new InputUnion { ki = new KEYBDINPUT { wScan = ch, dwFlags = KEYEVENTF_UNICODE } } },
                    new INPUT { type = INPUT_KEYBOARD, u = new InputUnion { ki = new KEYBDINPUT { wScan = ch, dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP } } }
                };
                SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());

                if (keyDownTimeMs > 0)
                    await Task.Delay(keyDownTimeMs, token).ConfigureAwait(false);
                if (debounceTimeMs > 0)
                    await Task.Delay(debounceTimeMs, token).ConfigureAwait(false);
            }
        }
    }
}
