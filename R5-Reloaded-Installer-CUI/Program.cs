﻿using R5_Reloaded_Installer_Library.Exclusive;
using R5_Reloaded_Installer_Library.Get;
using R5_Reloaded_Installer_Library.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace R5_Reloaded_Installer_CUI
{
    class Program
    {
        private static string FinalDirectoryName = "R5-Reloaded";
        private static string ScriptsDirectoryPath = Path.Combine("platform", "scripts");

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

            var applicationList = InstalledApps.DisplayNameList();
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

            ConsoleExpansion.LogWrite("Get the download location...");
            var detoursR5_path = WebGetLink.DetoursR5();
            var scriptsR5_path = WebGetLink.ScriptsR5();
            var apexClient_path = WebGetLink.ApexClient();

            ConsoleExpansion.LogWrite("Get the file size...");
            var DriveRoot = Path.GetPathRoot(DirectoryExpansion.RunningDirectoryPath);
            var DriveSize = FileExpansion.ByteToGByte(FileExpansion.GetDriveFreeSpace(DriveRoot));
            var FileSize = FileExpansion.ByteToGByte(
                FileExpansion.GetZipFileSize(detoursR5_path) + 
                FileExpansion.GetZipFileSize(scriptsR5_path) +
                FileExpansion.GetTorrentFileSize(apexClient_path));

            ConsoleExpansion.LogWrite("["+DriveRoot + "] Drive Size   >> " + DriveSize + " GB");
            ConsoleExpansion.LogWrite("Download File Size >> " + FileSize + " GB");
            if (FileSize > DriveSize)
            {
                ConsoleExpansion.LogError("There is not enough space on the destination drive to install the software.");
                ConsoleExpansion.Exit();
            }
            ConsoleExpansion.LogWrite("(OK)");

            ConsoleExpansion.LogWrite("Do you want to continue the installation ?");
            ConsoleExpansion.LogWrite("Installation takes about an hour.");
            if (ConsoleExpansion.ConsentInput())
            {
                ConsoleExpansion.LogWrite("Preparing...");
                string detoursR5FilePath, scriptsR5FilePath, apexClientFilePath;
                using (var download = new Download())
                {
                    download.WebClientReceives += new WebClientProcessEventHandler(WebClient_EventHandler);
                    download.Aria2ProcessReceives += new Aria2ProcessEventHandler(Aria2Process_EventHandler);
                    detoursR5FilePath = download.RunZip(detoursR5_path, "detours_r5");
                    scriptsR5FilePath = download.RunZip(scriptsR5_path, "scripts_r5");
                    ConsoleExpansion.WriteWidth('=', "Download with aria2");
                    apexClientFilePath = download.RunTorrent(apexClient_path, FinalDirectoryName);
                    ConsoleExpansion.WriteWidth('=');
                }
                ConsoleExpansion.LogWrite("The detours_r5 file is being moved.");
                DirectoryExpansion.MoveOverwrite(detoursR5FilePath, apexClientFilePath);
                ConsoleExpansion.LogWrite("The scripts_r5 file is being moved.");
                Directory.Move(scriptsR5FilePath, Path.Combine(apexClientFilePath, ScriptsDirectoryPath));
                ConsoleExpansion.LogWrite("The entire process has been completed!");
                ConsoleExpansion.LogWrite("Done.");
            }
            ConsoleExpansion.Exit();
        }

        private static float LogTimer = 0;
        private static void WebClient_EventHandler(object sender, DownloadProgressChangedEventArgs e)
        {
            LogTimer += R5_Reloaded_Installer_Library.Other.Time.deltaTime;
            var parcentage = e.ProgressPercentage;
            if (LogTimer > 0.1f || parcentage == 100)
            {
                LogTimer = 0;
                var data = (string[])sender;
                var fileName = Path.GetFileNameWithoutExtension(data[1]);
                var fileExt = Path.GetExtension(data[1]).Replace(".", string.Empty).ToUpper();
                var received = Math.Round(FileExpansion.ByteToKByte(e.BytesReceived) * 1000f) / 1000f;
                var total = Math.Round(FileExpansion.ByteToKByte(e.TotalBytesToReceive) * 1000f) / 1000f;

                if (parcentage == 100) Thread.Sleep(1);
                ConsoleExpansion.LogWrite(
                    "Download " + fileName + " (" + fileExt + ") >> " +
                    string.Format("{0,8}", received.ToString("0.000")) +
                    "KB/" + string.Format("{0,8}", total.ToString("0.000")) + 
                    "KB (" + string.Format("{0,3}", parcentage) + "%)");
                if (parcentage == 100) ConsoleExpansion.LogWrite("(OK)"); ;
            }
        }

        private static void Aria2Process_EventHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (string.IsNullOrEmpty(outLine.Data)) return;

            var rawLine = Regex.Replace(outLine.Data, @"(\r|\n|(  )|\t|\x1b\[.*?m)", string.Empty);

            if (rawLine[0] == '[')
            {
                var nakedLine = Regex.Replace(rawLine, @"((#.{6}( ))|\[|\])", "");
                if (rawLine.Contains("FileAlloc"))
                {
                    ConsoleExpansion.LogWrite(nakedLine.Substring(nakedLine.IndexOf("FileAlloc")));
                }
                else
                {
                    ConsoleExpansion.LogWrite(nakedLine);
                }
            }
            else if (rawLine[0] == '(')
            {
                ConsoleExpansion.LogWrite(rawLine);
            }
            else if (rawLine.Contains("NOTICE"))
            {
                var nakedLine = Regex.Replace(rawLine, @"([0-9]{2}/[0-9]{2})( )([0-9]{2}:[0-9]{2}:[0-9]{2})( )", string.Empty);
                ConsoleExpansion.LogWrite(nakedLine);
            }
        }
    }
}
