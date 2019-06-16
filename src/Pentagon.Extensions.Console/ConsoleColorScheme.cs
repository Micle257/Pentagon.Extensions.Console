// -----------------------------------------------------------------------
//  <copyright file="ConsoleColorScheme.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console
{
    using System;

    public class ConsoleColorScheme
    {
        public CliConsoleColor Success { get; set; } = new CliConsoleColor(ConsoleColor.Green);

        public CliConsoleColor Error { get; set; } = new CliConsoleColor(ConsoleColor.Red);

        public CliConsoleColor Warning { get; set; } = new CliConsoleColor(ConsoleColor.Yellow);

        public CliConsoleColor Info { get; set; } = new CliConsoleColor(ConsoleColor.Cyan);

        public CliConsoleColor Text { get; set; } = new CliConsoleColor(ConsoleColor.White);

        public CliConsoleColor MutedText { get; set; } = new CliConsoleColor(ConsoleColor.Gray);

        public CliConsoleColor InvertedText { get; set; } = new CliConsoleColor(ConsoleColor.Black, ConsoleColor.White);

        public CliConsoleColor Blank { get; set; } = new CliConsoleColor(ConsoleColor.Black, ConsoleColor.Black);
    }
}