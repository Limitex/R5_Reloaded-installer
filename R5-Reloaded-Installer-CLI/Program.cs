﻿using R5_Reloaded_Installer_Library.Exclusive;
using R5_Reloaded_Installer_Library.Get;
using R5_Reloaded_Installer_Library.IO;
using R5_Reloaded_Installer_Library.Text;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace R5_Reloaded_Installer_CLI
{
    enum ApexClientDownloadType
    {
        Binary,
        Torrent
    }

    class Program
    {
        private static string FinalDirectoryName = "R5-Reloaded";
        private static string ScriptsDirectoryPath = Path.Combine("platform", "scripts");
        private static string WorldsEdgeAfterDarkPath = "package";
        private static string DirectionPath = Path.GetDirectoryName(Environment.ProcessPath) ?? string.Empty;
        private static float AllAboutByteSize = 42f * 1024f * 1024f * 1024f;
        private static ApexClientDownloadType ACdownloadType;
        private static ApplicationType torrentAppType;
        private static ApplicationType fileAppType;

        static void Main(string[] args)
        {
            if (DirectionPath == string.Empty) throw new Exception();
            DirectionPath = Path.Combine(DirectionPath, FinalDirectoryName);

            ConsoleExpansion.DisableEasyEditMode();
            ConsoleExpansion.EnableVirtualTerminalProcessing();
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

            if (!(InstalledApps.DisplayNameList() ?? Array.Empty<string>()).Contains("Origin"))
            {
                ConsoleExpansion.LogError("\'Origin\' is not installed.");
                ConsoleExpansion.LogError("Do you want to continue?");
                ConsoleExpansion.LogError("R5 Reloaded cannot be run without \'Origin\' installed.");
                if (!ConsoleExpansion.ConsentInput()) ConsoleExpansion.Exit();
            }

            ConsoleExpansion.LogWrite("Get the download location...");
            var detoursR5_link = WebGetLink.DetoursR5();
            var scriptsR5_link = WebGetLink.ScriptsR5();
            var apexClient_link = WebGetLink.ApexClient_Torrent();
            var worldsEdgeAfterDark_link = WebGetLink.WorldsEdgeAfterDark();

            ConsoleExpansion.LogWrite("Get the file size...");
            var DriveRoot = Path.GetPathRoot(DirectionPath) ?? string.Empty;
            var DriveSize = FileExpansion.GetDriveFreeSpace(DriveRoot);
            ConsoleExpansion.LogWrite("[" + DriveRoot + "] Drive Size   >> " + StringProcessing.ByteToStringWithUnits(DriveSize));
            ConsoleExpansion.LogWrite("Download File Size >> " + StringProcessing.ByteToStringWithUnits(AllAboutByteSize));
            if (AllAboutByteSize > DriveSize)
            {
                ConsoleExpansion.LogError("There is not enough space on the destination drive to install the software.");
                ConsoleExpansion.LogWrite("Do you want to continue?");
                if (!ConsoleExpansion.ConsentInput()) ConsoleExpansion.Exit();
            }
            ConsoleExpansion.LogWrite("(OK)");

            ConsoleExpansion.LogWrite(string.Empty);
            ConsoleExpansion.LogWrite("Select the download destination for Apex Client.");
            ConsoleExpansion.LogInput("Enter 1 for Binary. Enter 2 for Torrent.");
            ConsoleExpansion.LogInput("If you select any other key, it is considered that you have selected \'Binary\'.");
            ConsoleExpansion.LogInput(" > ");
            ACdownloadType = (Console.ReadKey().KeyChar) switch
            {
                '1' => ApexClientDownloadType.Binary,
                '2' => ApexClientDownloadType.Torrent,
                _ => ApexClientDownloadType.Binary
            };

            if (ACdownloadType == ApexClientDownloadType.Torrent)
            {
                ConsoleExpansion.LogWrite(string.Empty);
                ConsoleExpansion.LogWrite("Select the software you want to use to download torrents.");
                ConsoleExpansion.LogInput("Enter 1 for Transmission. Enter 2 for Aria2.");
                ConsoleExpansion.LogInput("If you select any other key, it is considered that you have selected \'Transmission\'.");
                ConsoleExpansion.LogInput(" > ");
                torrentAppType = (Console.ReadKey().KeyChar) switch
                {
                    '1' => ApplicationType.Transmission,
                    '2' => ApplicationType.Aria2c,
                    _ => ApplicationType.Transmission
                };
            }

            ConsoleExpansion.LogWrite(string.Empty);
            ConsoleExpansion.LogWrite("Select the software you want to use to download the file.");
            ConsoleExpansion.LogInput("Enter 1 for HttpClient. Enter 2 for Aria2.");
            ConsoleExpansion.LogInput("If you select any other key, it is considered that you have selected \'HttpClient\'.");
            ConsoleExpansion.LogInput(" > ");
            fileAppType = (Console.ReadKey().KeyChar) switch
            {
                '1' => ApplicationType.HttpClient,
                '2' => ApplicationType.Aria2c,
                _ => ApplicationType.HttpClient
            };

            ConsoleExpansion.LogWrite(string.Empty);
            ConsoleExpansion.LogNotes("It takes about 40 minutes depending on the download destination.");
            ConsoleExpansion.LogNotes("Do not delete the directory generated by this program.");
            ConsoleExpansion.LogWrite("Do you want to continue the installation ?");
            
            ConsoleExpansion.LogNotes(string.Empty);
            ConsoleExpansion.LogNotes("Method used for each download.");
            ConsoleExpansion.LogNotes("\tWorlds edge after dark : " + fileAppType);
            ConsoleExpansion.LogNotes("\tdetours_r5             : " + fileAppType);
            ConsoleExpansion.LogNotes("\tscripts r5             : " + fileAppType);
            switch (ACdownloadType)
            {
                case ApexClientDownloadType.Binary:
                    ConsoleExpansion.LogNotes("\tApex Client Season 3   : " + fileAppType);
                    break;
                case ApexClientDownloadType.Torrent:
                    ConsoleExpansion.LogNotes("\tApex Client Season 3   : " + torrentAppType);
                    break;
            }

            ConsoleExpansion.LogNotes(string.Empty);
            if (!ConsoleExpansion.ConsentInput()) ConsoleExpansion.Exit();
            ConsoleExpansion.LogNotes(string.Empty);

            ConsoleExpansion.LogWrite("Preparing...");
            using (var download = new Download(DirectionPath))
            {
                download.ProcessReceives += (appType, outline) => ConsoleExpansion.LogWrite("(" + appType + ") " + outline);
                ConsoleExpansion.LogWrite("The download will start.");
                ConsoleExpansion.WriteWidth('=', "Downloading Worlds edge after dark");
                var worldsEdgeAfterDarkDirPath = download.Run(WebGetLink.WorldsEdgeAfterDark(), "WorldsEdgeAfterDark", appType: fileAppType);
                ConsoleExpansion.WriteWidth('=', "Downloading detours_r5");
                var detoursR5DirPath = download.Run(WebGetLink.DetoursR5(), "detoursR5", appType: fileAppType);
                ConsoleExpansion.WriteWidth('=', "Downloading scripts r5");
                var scriptsR5DirPath = download.Run(WebGetLink.ScriptsR5(), "scriptsR5", appType: fileAppType);
                ConsoleExpansion.WriteWidth('=', "Downloading Apex Client Season 3");

                string apexClientDirPath = string.Empty;
                switch (ACdownloadType)
                {
                    case ApexClientDownloadType.Binary:
                        apexClientDirPath = download.Run(WebGetLink.ApexClient_Binary(), "ApexClient", appType: fileAppType);
                        break;
                    case ApexClientDownloadType.Torrent:
                        apexClientDirPath = download.Run(WebGetLink.ApexClient_Torrent(), "ApexClient", appType: torrentAppType);
                        break;
                }
                ConsoleExpansion.WriteWidth('=');

                ConsoleExpansion.LogWrite("Creating the R5-Reloaded");
                DirectoryExpansion.MoveOverwrite(detoursR5DirPath, apexClientDirPath);
                Directory.Move(scriptsR5DirPath, Path.Combine(apexClientDirPath, ScriptsDirectoryPath));
                DirectoryExpansion.MoveOverwrite(Path.Combine(worldsEdgeAfterDarkDirPath, WorldsEdgeAfterDarkPath), apexClientDirPath);
                DirectoryExpansion.DirectoryDelete(worldsEdgeAfterDarkDirPath);
                download.DirectoryFix(DirectionPath);
                ConsoleExpansion.LogWrite("Done.");
            }
            ConsoleExpansion.Exit();
        }
    }
}