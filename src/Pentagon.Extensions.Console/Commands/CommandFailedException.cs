namespace Pentagon.Extensions.Console {
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class CommandFailedException : Exception
    {
        public string FileName { get; }
        public string Args { get; }
        public string ErrorOutput { get; }
        public int ExitCode { get; }

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
    }
}