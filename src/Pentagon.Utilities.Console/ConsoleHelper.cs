// -----------------------------------------------------------------------
//  <copyright file="ConsoleHelper.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Helpers
{
    using System;
    using System.IO;

    public static class ConsoleHelper
    {
        public static void Write(object value, ConsoleColor? foreColor, ConsoleColor? backColor = null)
        {
            var fore = Console.ForegroundColor;
            var back = Console.BackgroundColor;
            if (foreColor.HasValue)
                Console.ForegroundColor = foreColor.Value;
            if (backColor.HasValue)
                Console.BackgroundColor = backColor.Value;
            Console.Write(value);
            if (foreColor.HasValue)
                Console.ForegroundColor = fore;
            if (backColor.HasValue)
                Console.BackgroundColor = back;
        }

        public static void WriteLine(object value, ConsoleColor? foreColor, ConsoleColor? backColor = null)
        {
            var fore = Console.ForegroundColor;
            var back = Console.BackgroundColor;
            if (foreColor.HasValue)
                Console.ForegroundColor = foreColor.Value;
            if (backColor.HasValue)
                Console.BackgroundColor = backColor.Value;
            Console.WriteLine(value);
            if (foreColor.HasValue)
                Console.ForegroundColor = fore;
            if (backColor.HasValue)
                Console.BackgroundColor = back;
        }

        public static void WriteSuccess(object successValue) => Write(successValue, ConsoleColor.Green);

        public static void WriteError(object errorValue) => Write(errorValue, ConsoleColor.Red);

        public static void WriteWarning(object warningValue) => Write(warningValue, ConsoleColor.Yellow);

        public static void PlayWavFile(string wavFilePath, bool writeOutput = false)
        {
            switch (OS.Platform)
            {
                case OperatingSystemPlatform.Linux:
                    var output = ShellHelper.RunCommand($"aplay {wavFilePath}");
                    if (writeOutput)
                        Console.WriteLine(output);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}