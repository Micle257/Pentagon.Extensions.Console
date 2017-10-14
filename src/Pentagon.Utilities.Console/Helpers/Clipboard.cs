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
            var os = new OperatingSystem();
            os.Initialize();

            if (os.Platform == OSPlatform.Windows)
                Shell.Bat($"echo|set /p={value} | clip");
            else if (os.Platform == OSPlatform.OSX)
                Shell.Bash($"echo \"{value}\" | pbcopy");
        }

        public static string GetText()
        {
            var os = new OperatingSystem();
            os.Initialize();

            if (os.Platform == OSPlatform.Windows)
                return Shell.Bat(command: "pclip");

            throw new ArgumentException();
        }
    }
}