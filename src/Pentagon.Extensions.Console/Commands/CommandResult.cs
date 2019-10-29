// -----------------------------------------------------------------------
//  <copyright file="CommandResult.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Commands
{
    using System.Diagnostics;
    using OperationResults;

    public class CommandResult : OperationResult<string>
    {
        public int? ExitCode => Process?.ExitCode;

        public string ErrorMessage { get; set; }

        public Process Process { get; set; }
    }
}