using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SleepOnLanLibrary
{
    public static class AutoStartManager
    {
        private static readonly string ProgramRegistryName = "SleepOnLAN";
        private static readonly string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        public static void SetAutoStart(bool Enabled)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            if (Enabled)
            {
                key.SetValue(ProgramRegistryName, Assembly.GetEntryAssembly().Location);
            }
            else
            {
                key.DeleteValue(ProgramRegistryName, false);
            }
        }

        public static bool IsAutoStartEnabled()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);

            return key.GetValueNames().Contains(ProgramRegistryName);
        }
    }
}
