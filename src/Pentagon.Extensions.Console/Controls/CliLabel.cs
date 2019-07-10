// -----------------------------------------------------------------------
//  <copyright file="CliLabel.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Controls
{
    using System;

    public class CliLabel
    {
        public char? Prefix { get; set; }

        public CliConsoleColor PrefixColor { get; set; }

        public CliConsoleColor DescriptionColor { get; set; }

        public void Write(string description)
        {
            ConsoleHelper.EnsureNewLine();

            if (Prefix.HasValue)
            {
                ConsoleWriter.Write(Prefix.Value, PrefixColor);

                ConsoleWriter.Write(new string(' ', 1));
            }

            if (description != null)
                ConsoleWriter.Write(description, DescriptionColor);
        }
    }
}