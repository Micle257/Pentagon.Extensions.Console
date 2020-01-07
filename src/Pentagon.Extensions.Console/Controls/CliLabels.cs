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
                                                   DescriptionColor = CliContext.ColorScheme.Text
                                           };

        [NotNull]
        public static CliLabel Success = new CliLabel
                                         {
                                                 Prefix = '+',
                                                 PrefixColor = CliContext.ColorScheme.Success,
                                                 DescriptionColor = CliContext.ColorScheme.Text
                                         };

        [NotNull]
        public static CliLabel Info = new CliLabel
                                      {
                                              Prefix = 'i',
                                              PrefixColor = CliContext.ColorScheme.Info,
                                              DescriptionColor = CliContext.ColorScheme.Text
                                      };

        [NotNull]
        public static CliLabel Error = new CliLabel
                                       {
                                               Prefix = 'x',
                                               PrefixColor = CliContext.ColorScheme.Error,
                                               DescriptionColor = CliContext.ColorScheme.Text
                                       };

        [NotNull]
        public static CliLabel Warning = new CliLabel
                                         {
                                                 Prefix = '!',
                                                 PrefixColor = CliContext.ColorScheme.Warning,
                                                 DescriptionColor = CliContext.ColorScheme.Text
                                         };
    }
}