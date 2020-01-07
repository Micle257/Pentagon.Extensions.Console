namespace Pentagon.Extensions.Console.Cli {
    using System.Collections.Generic;
    using System.CommandLine;
    using System.Linq;

    public class CliCommandInfo
    {
        public ICommand Command { get; }

        public CliCommandDescriber Describer { get; }

        public IReadOnlyCollection<CliOptionInfo> Options { get; }

        public IReadOnlyCollection<CliArgumentInfo> Arguments { get; }

        public CliCommandInfo(ICommand command, CliCommandDescriber describer, IEnumerable<CliOptionInfo> options, IEnumerable<CliArgumentInfo> arguments)
        {
            Command   = command;
            Describer = describer;
            Options   = options.ToList().AsReadOnly();
            Arguments = arguments.ToList().AsReadOnly();
        }
    }
}