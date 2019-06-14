// -----------------------------------------------------------------------
//  <copyright file="Clipboard.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console
{
    using System;
    using Commands;

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
                    return ShellHelper.Batch(command: "pclip").Content;
            }

            throw new NotSupportedException();
        }
    }
}