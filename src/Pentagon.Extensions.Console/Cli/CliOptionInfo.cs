namespace Pentagon.Extensions.Console.Cli {
    using System.CommandLine;

    public class CliOptionInfo {
        public IOption Option { get; }
        public CliOptionDescriber Describer { get; }

        public CliOptionInfo(IOption option, CliOptionDescriber describer)
        {
            Option    = option;
            Describer = describer;
        }
    }
}