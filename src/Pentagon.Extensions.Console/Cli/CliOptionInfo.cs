// -----------------------------------------------------------------------
//  <copyright file="CliOptionInfo.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System.CommandLine;

    public class CliOptionInfo
    {
        public CliOptionInfo(IOption option, CliOptionDescriber describer)
        {
            Option    = option;
            Describer = describer;
        }

        public IOption Option { get; }
        public CliOptionDescriber Describer { get; }
    }
}