// -----------------------------------------------------------------------
//  <copyright file="CliLabels.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Controls
{
    using System;
    using JetBrains.Annotations;

    public static class CliLabels
    {
        [NotNull]
        public static CliLabel FormField = new CliLabel
                                           {
                                                   Prefix = '?',
                                                   PrefixColor = new CliConsoleColor(ConsoleColor.DarkGreen),
                                                   DescriptionColor = Cli.ColorScheme.Text
                                           };

        [NotNull]
        public static CliLabel Success = new CliLabel
                                         {
                                                 Prefix = '+',
                                                 PrefixColor = Cli.ColorScheme.Success,
                                                 DescriptionColor = Cli.ColorScheme.Text
                                         };

        [NotNull]
        public static CliLabel Info = new CliLabel
                                      {
                                              Prefix = 'i',
                                              PrefixColor = Cli.ColorScheme.Info,
                                              DescriptionColor = Cli.ColorScheme.Text
                                      };

        [NotNull]
        public static CliLabel Error = new CliLabel
                                       {
                                               Prefix = 'x',
                                               PrefixColor = Cli.ColorScheme.Error,
                                               DescriptionColor = Cli.ColorScheme.Text
                                       };

        [NotNull]
        public static CliLabel Warning = new CliLabel
                                         {
                                                 Prefix = '!',
                                                 PrefixColor = Cli.ColorScheme.Warning,
                                                 DescriptionColor = Cli.ColorScheme.Text
                                         };
    }
}