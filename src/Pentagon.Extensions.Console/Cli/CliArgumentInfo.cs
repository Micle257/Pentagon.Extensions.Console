namespace Pentagon.Extensions.Console.Cli {
    using System.CommandLine;

    public class CliArgumentInfo {
        public IArgument Argument { get; }
        public CliArgumentDescriber Describer { get; }

        public CliArgumentInfo(IArgument argument, CliArgumentDescriber describer)
        {
            Argument  = argument;
            Describer = describer;
        }
    }
}