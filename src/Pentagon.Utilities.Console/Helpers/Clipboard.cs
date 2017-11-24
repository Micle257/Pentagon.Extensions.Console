// -----------------------------------------------------------------------
//  <copyright file="Clipboard.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Helpers
{
    using System;
    using System.Runtime.InteropServices;
    using OperatingSystem = Console.OperatingSystem;

    public static class Clipboard
    {
        public static void Copy(string value)
        {
            switch (OperatingSystem.Platform)
            {
                case OperatingSystemPlatform.Windows:
                    Shell.Bat($"echo|set /p={value} | clip");
                    break;

                case OperatingSystemPlatform.OSX:
                    Shell.Bash($"echo \"{value}\" | pbcopy");
                    break;
            }
        }

        public static string GetText()
        {
            switch (OperatingSystem.Platform)
            {
                case OperatingSystemPlatform.Windows:
                    return Shell.Bat(command: "pclip");
            }

            throw new NotSupportedException();
        }
    }
}