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
                                                   DescriptionColor = ConsoleHelper.ColorScheme.Text
                                           };

        [NotNull]
        public static CliLabel Success = new CliLabel
                                         {
                                                 Prefix = '+',
                                                 PrefixColor = ConsoleHelper.ColorScheme.Success,
                                                 DescriptionColor = ConsoleHelper.ColorScheme.Text
                                         };

        [NotNull]
        public static CliLabel Info = new CliLabel
                                      {
                                              Prefix = 'i',
                                              PrefixColor = ConsoleHelper.ColorScheme.Info,
                                              DescriptionColor = ConsoleHelper.ColorScheme.Text
                                      };

        [NotNull]
        public static CliLabel Error = new CliLabel
                                       {
                                               Prefix = 'x',
                                               PrefixColor = ConsoleHelper.ColorScheme.Error,
                                               DescriptionColor = ConsoleHelper.ColorScheme.Text
                                       };

        [NotNull]
        public static CliLabel Warning = new CliLabel
                                         {
                                                 Prefix = '!',
                                                 PrefixColor = ConsoleHelper.ColorScheme.Warning,
                                                 DescriptionColor = ConsoleHelper.ColorScheme.Text
                                         };
    }
}