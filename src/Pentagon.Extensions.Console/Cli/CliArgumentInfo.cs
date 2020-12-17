// -----------------------------------------------------------------------
//  <copyright file="CliArgumentInfo.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System.CommandLine;

    public class CliArgumentInfo
    {
        public CliArgumentInfo(IArgument argument, CliArgumentDescriber describer)
        {
            Argument  = argument;
            Describer = describer;
        }

        public IArgument Argument { get; }
        public CliArgumentDescriber Describer { get; }
    }
}