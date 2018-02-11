// -----------------------------------------------------------------------
//  <copyright file="Clipboard.cs">
//   Copyright (c) Michal Pokorn�. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Helpers
{
    using System;

    public static class Clipboard
    {
        public static void Copy(string value)
        {
            switch (OS.Platform)
            {
                case OperatingSystemPlatform.Windows:
                    ShellHelper.Batch($"echo|set /p={value} | clip");
                    break;

                case OperatingSystemPlatform.OSX:
                    ShellHelper.Bash($"echo \"{value}\" | pbcopy");
                    break;
            }
        }

        public static string GetText()
        {
            switch (OS.Platform)
            {
                case OperatingSystemPlatform.Windows:
                    return ShellHelper.Batch(command: "pclip");
            }

            throw new NotSupportedException();
        }
    }
}