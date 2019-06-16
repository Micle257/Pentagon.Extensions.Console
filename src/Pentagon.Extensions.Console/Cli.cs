// -----------------------------------------------------------------------
//  <copyright file="Cli.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console
{
    using System;
    using JetBrains.Annotations;

    public class Cli
    {
        [NotNull]
        public static ConsoleColorScheme ColorScheme { get; } = new ConsoleColorScheme();

        public Cli ModifyColorScheme(Action<ConsoleColorScheme> configure)
        {
            configure?.Invoke(ColorScheme);

            return this;
        }
    }
}