﻿using System;
using System.IO;

namespace R5_Reloaded_Installer
{
    class Program
    {
        private static string FinalDirectoryName = "R5-Reloaded";
        static void Main(string[] args)
        {
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

            FileOperations.SetDownloadLink();

            ConsoleExpansion.LogWriteLine("Do you want to continue the installation ?");
            ConsoleExpansion.LogWriteLine("Installation takes about an hour.");

            if (ConsoleExpansion.ConsentInput()) Install_R5();

            ConsoleExpansion.ExitConsole();
        }

        private static void Install_R5()
        {
            FileOperations.DownloadFiles();

            ConsoleExpansion.LogWriteLine("Moving " + FileOperations.FileName_detours + " to APEX Client.");
            DirectoryExpansion.AllMove(FileOperations.FileName_detours, FileOperations.FileName_apex);
            ConsoleExpansion.LogWriteLine("Creating and moving the script directory.");
            Directory.CreateDirectory(FileOperations.R5_ScriptsPath);
            DirectoryExpansion.AllMove(FileOperations.FileName_scripts, FileOperations.R5_ScriptsPath);

            ConsoleExpansion.LogWriteLine("The end process is in progress.");
            Directory.Move(FileOperations.FileName_apex, FinalDirectoryName);

            ConsoleExpansion.LogWriteLine("Exists in the " + FinalDirectoryName + " directory.");
            ConsoleExpansion.LogWriteLine("Done.");
        }
    }
}
