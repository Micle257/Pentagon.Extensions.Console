namespace Pentagon.Extensions.Console.Commands {
    using System.Diagnostics;

    public class CommandResult : OperationResult<string>
    {
        public int? ExitCode => Process?.ExitCode;

        public Process Process { get; set; }
    }
}