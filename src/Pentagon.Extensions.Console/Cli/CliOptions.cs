// -----------------------------------------------------------------------
//  <copyright file="CliOptions.cs">
//   Copyright (c) Michal Pokorn�. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    public class CliOptions
    {
        public bool InvokeAllMatchedHandlers { get; set; } = false;

        public bool ExitOnError { get; set; } = true;

        public bool UseAnnotatedCommands { get; set; } = true;
    }
}