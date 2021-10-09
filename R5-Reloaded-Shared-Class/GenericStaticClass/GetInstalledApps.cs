﻿using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace R5_Reloaded_Installer.SharedClass
{
    public class GetInstalledApps
    {
        private static string RegistryPath_64 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        private static string RegistryPath_32 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

        public static string[] AllList()
        {
            var List64 = GetUninstallList(RegistryPath_64);
            var List32 = GetUninstallList(RegistryPath_32);
            var ListAll = List64.Concat(List32).ToArray();
            Array.Sort(ListAll, StringComparer.OrdinalIgnoreCase);
            return ListAll;
        }

        private static List<string> GetUninstallList(string path)
        {
            var nameList = new List<string>();
            foreach (var subKey in Registry.LocalMachine.OpenSubKey(path, false).GetSubKeyNames())
            {
                var subKeys = Registry.LocalMachine.OpenSubKey(path + @"\" + subKey, false);
                var displayName = subKeys.GetValue("DisplayName");
                if (displayName != null) nameList.Add(displayName.ToString());
                else nameList.Add(subKey);
            }
            return nameList;
        }

        public static void CreateShortcut(string path, string name, string LinkDestination, string arguments)
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(Path.Combine(path, name + @".lnk"));
            shortcut.TargetPath = LinkDestination;
            shortcut.Arguments = arguments;
            shortcut.WorkingDirectory = Path.GetDirectoryName(LinkDestination);
            shortcut.WindowStyle = 1;
            shortcut.IconLocation = LinkDestination + ",0";
            shortcut.Save();
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
        }
    }
}
