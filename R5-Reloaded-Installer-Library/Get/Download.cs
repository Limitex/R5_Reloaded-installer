﻿using R5_Reloaded_Installer_Library.External;
using R5_Reloaded_Installer_Library.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace R5_Reloaded_Installer_Library.Get
{
    public enum ApplicationType
    {
        Aria2c,
        Transmission,
        SevenZip
    }

    public delegate void ProcessEventHandler(ApplicationType appType, string outLine);

    public class Download : IDisposable
    {
        public event ProcessEventHandler? ProcessReceives = null;

        private static string WorkingDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "R5-Reloaded-Installer");
        private string SaveingDirectoryPath;

        private ResourceProcess aria2c;
        private ResourceProcess sevenZip;
        private ResourceProcess transmission;

        public Download(string saveingDirectoryPath)
        {
            SaveingDirectoryPath = saveingDirectoryPath;
            DirectoryExpansion.CreateOverwrite(WorkingDirectoryPath);

            aria2c = new ResourceProcess(WorkingDirectoryPath, "aria2c");
            sevenZip = new ResourceProcess(WorkingDirectoryPath, "seven-za");
            transmission = new ResourceProcess(WorkingDirectoryPath, "transmission");

            aria2c.ResourceProcessReceives += new ResourceProcessEventHandler(Aria2cProcess_EventHandler);
            sevenZip.ResourceProcessReceives += new ResourceProcessEventHandler(SevenZipProcess_EventHandler);
            transmission.ResourceProcessReceives += new ResourceProcessEventHandler(TransmissionProcess_EventHandler);
        }

        public void Dispose()
        {
            aria2c.Dispose();
            sevenZip.Dispose();
            transmission.Dispose();
            DirectoryExpansion.DirectoryDelete(WorkingDirectoryPath);
        }

        public string Run(string address, string? name = null, string? path = null, ApplicationType? appType = null)
        {
            switch (Path.GetExtension(address).ToLower())
            {
                case ".zip":
                case ".7z":
                    if(appType != null) throw new Exception("The app type can only be specified for torrents.");
                    var filePath = Aria2c(address, name, path);
                    var dirPath = SevenZip(filePath, name, path);
                    DirectoryFix(dirPath);
                    return dirPath;
                case ".torrent":
                    string torrentdirPath;
                    switch (appType) 
                    {
                        case ApplicationType.Aria2c:
                        case null:
                            torrentdirPath = Aria2c(address, name, path);
                            break;
                        case ApplicationType.Transmission:
                            torrentdirPath = Transmission(address, name, path);
                            break;
                        default:
                            throw new Exception("Specify \"aria2c\" or \"transmission\" for the app type.");
                    }
                    DirectoryFix(torrentdirPath);
                    return torrentdirPath;
                default:
                    throw new Exception("The specified address cannot be downloaded with.");
            }
        }

        private string Aria2c(string address, string? name = null, string? path = null)
        {
            var extension = Path.GetExtension(address);
            var fileName = name == null ? Path.GetFileName(address) : name + extension;
            var dirPath = path ?? SaveingDirectoryPath;
            var dirName = Path.GetFileNameWithoutExtension(fileName);
            var resurtPath = Path.Combine(dirPath, dirName);
            var argument = " --dir=\"" + resurtPath + "\" --out=\"" + fileName + "\" --seed-time=0 --allow-overwrite=true --follow-torrent=mem";
            DirectoryExpansion.CreateOverwrite(resurtPath);
            aria2c.Run(address + argument, resurtPath);
            return extension != ".torrent" ? Path.Combine(resurtPath, fileName) : resurtPath;
        }

        private string Transmission(string address, string? name = null, string? path = null)
        {
            var dirPath = path ?? SaveingDirectoryPath;
            var dirName = name ?? Path.GetFileNameWithoutExtension(address);
            var resurtPath = Path.Combine(dirPath, dirName);
            var argument = " --download-dir \"" + resurtPath + "\" --config-dir \"" + WorkingDirectoryPath + "\" -u 0";
            DirectoryExpansion.CreateOverwrite(resurtPath);
            transmission.Run(address + argument, dirPath);
            return resurtPath;
        }

        private string SevenZip(string address, string? name = null, string? path = null)
        {
            var dirPath = path ?? SaveingDirectoryPath;
            var dirName = name ?? Path.GetFileNameWithoutExtension(address);
            var resurtPath = Path.Combine(dirPath, dirName);
            var argument = "x -y \"" + address + "\" -o\"" + resurtPath + "\"";
            DirectoryExpansion.CreateIfNotFound(resurtPath);
            sevenZip.Run(argument, resurtPath);
            File.Delete(address);
            return resurtPath;
        }

        private void DirectoryFix(string sourceDirName)
        {
            var files = Directory.GetFiles(sourceDirName);
            var dirs = Directory.GetDirectories(sourceDirName);
            if (files.Length == 0 && dirs.Length == 1)
            {
                Directory.Move(dirs[0], sourceDirName + "_buffer");
                Directory.Delete(sourceDirName);
                Directory.Move(sourceDirName + "_buffer", sourceDirName);
            }
        }

        private string FormattingLine(string str) => Regex.Replace(str, @"(\r|\n|(  )|\t|\x1b\[.*?m)", string.Empty);

        private void Aria2cProcess_EventHandler(object sender, DataReceivedEventArgs outLine)
        {
            if (ProcessReceives == null) return;
            if (string.IsNullOrEmpty(outLine.Data)) return;
            var rawLine = FormattingLine(outLine.Data);

            if (rawLine[0] == '[')
            {
                var nakedLine = Regex.Replace(rawLine, @"((#.{6}( ))|\[|\])", "");
                if (rawLine.Contains("FileAlloc"))
                    ProcessReceives(ApplicationType.Aria2c ,nakedLine.Substring(nakedLine.IndexOf("FileAlloc")));
                else
                    ProcessReceives(ApplicationType.Aria2c, nakedLine);
            }
            else if (rawLine[0] == '(')
            {
                ProcessReceives(ApplicationType.Aria2c, rawLine);
            }
            else if (rawLine.Contains("NOTICE"))
            {
                var nakedLine = Regex.Replace(rawLine, @"([0-9]{2}/[0-9]{2})( )([0-9]{2}:[0-9]{2}:[0-9]{2})( )", string.Empty);
                ProcessReceives(ApplicationType.Aria2c, nakedLine);
            }
        }

        private void SevenZipProcess_EventHandler(object sender, DataReceivedEventArgs outLine)
        {
            if (ProcessReceives == null) return;
            if (string.IsNullOrEmpty(outLine.Data)) return;
            var rawLine = FormattingLine(outLine.Data);

            ProcessReceives(ApplicationType.SevenZip, rawLine);
        }

        private void TransmissionProcess_EventHandler(object sender, DataReceivedEventArgs outLine)
        {
            if (ProcessReceives == null) return;
            if (string.IsNullOrEmpty(outLine.Data)) return;
            var rawLine = FormattingLine(outLine.Data);

            if (rawLine.Contains("Seeding"))
            {
                transmission.Kill();
                return;
            }

            var nakedLine = Regex.Replace(rawLine, @"(\[([0-9]{4})-([0-9]{2})-([0-9]{2})( )([0-9]{2}):([0-9]{2}):([0-9]{2})\.(.*?)\])( )", string.Empty);
            
            var dirName = Path.GetFileNameWithoutExtension(Regex.Match(((string[])sender)[0], "http.*?(?=( ))").ToString());
            if (!Regex.Match(nakedLine, dirName + ":").Success)
            {
                ProcessReceives(ApplicationType.Transmission, Regex.Replace(nakedLine, @", ul to 0 \(0 kB/s\) \[(0\.00|None)\]", string.Empty));
                Thread.Sleep(200);
            }
        }
    }
}
