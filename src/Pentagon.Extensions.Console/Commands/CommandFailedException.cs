// -----------------------------------------------------------------------
//  <copyright file="CommandFailedException.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Commands
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class CommandFailedException : Exception
    {
        public CommandFailedException(string fileName, string args, string errorOutput, int exitCode) : base($"Running command failed ({exitCode}): {errorOutput}")
        {
            FileName = fileName;
            Args = args;
            ErrorOutput = errorOutput;
            ExitCode = exitCode;
        }

        protected CommandFailedException(
                SerializationInfo info,
                StreamingContext context) : base(info, context) { }

        public string FileName { get; }
        public string Args { get; }
        public string ErrorOutput { get; }
        public int ExitCode { get; }
    }
}