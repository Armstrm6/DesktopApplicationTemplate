using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Helpers
{
    public static class AutoStartHelper
    {
        private const string AppName = "DesktopApplicationTemplate";
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static void EnableAutoStart()
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true)
                                     ?? Registry.CurrentUser.CreateSubKey(RegistryKeyPath)!;

            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(exePath))
                key.SetValue(AppName, $"\"{exePath}\"");
        }

        public static void DisableAutoStart()
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            key?.DeleteValue(AppName, false);
        }

        public static bool IsAutoStartEnabled()
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            string? value = key?.GetValue(AppName) as string;
            return !string.IsNullOrEmpty(value);
        }
    }
}
