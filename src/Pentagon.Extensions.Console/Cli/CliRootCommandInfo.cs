// -----------------------------------------------------------------------
//  <copyright file="CliRootCommandInfo.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System.Collections.Generic;
    using System.CommandLine;

    public class CliRootCommandInfo : CliCommandInfo
    {
        public CliRootCommandInfo(RootCommand command, CliCommandDescriber describer, IEnumerable<CliOptionInfo> options, IEnumerable<CliArgumentInfo> arguments) : base(command, describer, options, arguments)
        {
            Command = command;
        }

        public new RootCommand Command { get; }
    }
}