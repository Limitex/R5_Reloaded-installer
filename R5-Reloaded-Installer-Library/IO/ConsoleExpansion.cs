﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace R5_Reloaded_Installer_Library.IO
{
    public static class ConsoleExpansion
    {
        private static readonly int InformationMaxWidth = 5;
        private static readonly int ConsentMaxAttempts = 5;

        [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll")] private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
        [DllImport("kernel32.dll")] private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        private static readonly Hashtable ColorEscape = new()
        {
            ["Black"] = "\x1b[30m",
            ["Red"] = "\x1b[31m",
            ["Green"] = "\x1b[32m",
            ["Yellow"] = "\x1b[33m",
            ["Blue"] = "\x1b[34m",
            ["Magenta"] = "\x1b[35m",
            ["Cyan"] = "\x1b[36m",
            ["White"] = "\x1b[37m",
            ["Default"] = "\x1b[39m",
        };

        private static string LogInfo(string info, string color, string value) =>
            ColorEscape["Magenta"] + "[ " +
            ColorEscape[color] + info.PadRight(InformationMaxWidth) +
            ColorEscape["Magenta"] + " ][ " +
            ColorEscape["White"] +
                DateTime.Now.ToString("yyyy/MM/dd") + " " +
                DateTime.Now.ToString("HH:mm:ss") +
            ColorEscape["Magenta"] + " ]" +
            ColorEscape["Default"] + " : " + value;

        public static void LogWrite(string value) => Console.WriteLine(LogInfo("Info", "Green", value));
        public static void LogNotes(string value) => Console.WriteLine(LogInfo("Notes", "Yellow", value));
        public static void LogError(string value) => Console.WriteLine(LogInfo("Error", "Red", value));
        public static void LogDebug(string value) => Console.WriteLine(LogInfo("Debug", "Blue", value));
        public static void LogInput(string value) => Console.Write(LogInfo("Input", "Cyan", value));

        public static bool ConsentInput(string? CanselMassage = null)
        {
            var ConsentAttempts = 0;
            while (ConsentAttempts < ConsentMaxAttempts)
            {
                LogInput("Yes No (y/n) : ");
                ConsoleKey key;
                do key = Console.ReadKey().Key;
                while (key == ConsoleKey.Escape);
                Console.WriteLine();
                switch (key)
                {
                    case ConsoleKey.Y:
                        return true;
                    case ConsoleKey.N:
                        LogWrite(CanselMassage ?? "The operation was canceled by the user.");
                        return false;
                    default:
                        ConsentAttempts++;
                        if (ConsentAttempts < ConsentMaxAttempts)
                            LogError("Enter either Y or N. Type it again.");
                        break;
                }
            }
            LogError("Max attempts has been reached.");
            Exit();
            return false;
        }

        public static void WriteWidth(char c, string? text = null)
        {
            var outString = "";
            if (text == null)
                for (int i = 0; i < Console.WindowWidth; i++) outString += c;
            else
            {
                var size = (Console.WindowWidth / 2f) - (text.Length / 2f) - 2f;
                for (int i = 0; i <= size; i++) outString += c;
                outString += ' ' + text + ' ';
                for (int i = 0; i <= size; i++) outString += c;
            }
            Console.Write('\n' + outString + '\n');
        }

        public static void DisableEasyEditMode()
        {
            const int STD_INPUT_HANDLE = -10;
            const uint ENABLE_QUICK_EDIT = 0x0040;

            var consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
            GetConsoleMode(consoleHandle, out uint consoleMode);
            SetConsoleMode(consoleHandle, consoleMode & ~ENABLE_QUICK_EDIT);
        }

        public static void Exit()
        {
            Console.WriteLine();
            Console.WriteLine("Press the key to exit.");
            Console.ReadKey();
            Environment.Exit(0x8020);
        }
    }
}
