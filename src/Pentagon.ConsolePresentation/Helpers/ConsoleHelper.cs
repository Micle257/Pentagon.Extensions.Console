// -----------------------------------------------------------------------
//  <copyright file="ConsoleHelper.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Helpers
{
    using System;
    using System.IO;
    using Native;
    using Structures;

    public static class ConsoleHelper
    {
        public static void Run(Action action)
        {
            try
            {
                action();
            }
            catch (IOException e)
            {
                var err = KernelNativeMethods.GetLastError();
            }
            catch (UnauthorizedAccessException e)
            {
                var err = KernelNativeMethods.GetLastError();
            }
        }

        public static void SetColor(ConsoleColour color, IConsoleColorProvider colorProvider)
        {
            var colorForeground = (color.Foreground ?? colorProvider.TextColor.Foreground).ToConsoleColor();
            if (color.IsBlank)
                colorForeground = colorProvider.BlankColor.Foreground.ToConsoleColor();
            else if (color.IsText)
                colorForeground = colorProvider.TextColor.Foreground.ToConsoleColor();

            var colorBackground = (color.Background ?? colorProvider.TextColor.Background).ToConsoleColor();
            if (color.IsBlank)
                colorBackground = colorProvider.BlankColor.Background.ToConsoleColor();
            else if (color.IsText)
                colorBackground = colorProvider.TextColor.Background.ToConsoleColor();

            Run(() =>
                {
                    Console.ForegroundColor = colorForeground;
                    Console.BackgroundColor = colorBackground;
                });
        }
    }
}