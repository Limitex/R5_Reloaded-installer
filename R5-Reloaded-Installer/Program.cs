﻿using System;
using System.IO;
using System.Linq;
using R5_Reloaded_Installer.SharedClass;

namespace R5_Reloaded_Installer
{
    class Program
    {
        private static string FinalDirectoryName = "R5-Reloaded";
        private static string ScriptsDirectoryPath = Path.Combine(FinalDirectoryName, "platform", "scripts");
        static void Main(string[] args)
        {
            ConsoleExpansion.DisableEasyEditMode();

            Console.WriteLine("\n" +
                "  -----------------------------------\n" +
                "  ||                               ||\n" +
                "  ||     R5-Reloaded Installer     ||\n" +
                "  ||                               ||\n" +
                "  -----------------------------------\n\n" +
                "  This program was created by Limitex.\n" +
                "  Please refer to the link below for the latest version of this program.\n\n" +
                "  https://github.com/Limitex/R5-Reloaded-Installer/releases \n\n" +
                "Welcome!\n");

            var applicationList = GetInstalledApps.AllList();
            if (!(applicationList.Contains("Origin") && applicationList.Contains("Apex Legends")))
            {
                ConsoleExpansion.LogError("\'Origin\' or \'Apex Legends\' is not installed.");
                ConsoleExpansion.LogError("Do you want to continue?");
                ConsoleExpansion.LogError("R5 Reloaded cannot be run without \'Origin\' and \'Apex Legends\' installed.");
                if (!ConsoleExpansion.ConsentInput())
                {
                    ConsoleExpansion.Exit();
                }
            }

            ConsoleExpansion.LogWrite("Do you want to continue the installation ?");
            ConsoleExpansion.LogWrite("Installation takes about an hour.");
            if (ConsoleExpansion.ConsentInput())
            {
                string detoursR5FileName, scriptsR5FileName;
                using (new Download())
                {
                    detoursR5FileName = Download.RunZip(WebGetLink.GetDetoursR5Link(), directoryName: "detours_r5");
                    scriptsR5FileName = Download.RunZip(WebGetLink.GetScriptsR5Link(), directoryName: "scripts_r5");
                    Download.RunTorrent(WebGetLink.GetApexClientLink(), directoryName: FinalDirectoryName);
                }
                ConsoleExpansion.LogWrite("The detours_r5 file is being moved.");
                DirectoryExpansion.MoveOverwrite(detoursR5FileName, FinalDirectoryName);
                ConsoleExpansion.LogWrite("The scripts_r5 file is being moved.");
                Directory.Move(scriptsR5FileName, ScriptsDirectoryPath);
                ConsoleExpansion.LogWrite("The entire process has been completed!");
                ConsoleExpansion.LogWrite("Done.");
            }
            ConsoleExpansion.Exit();
        }

    }
}
