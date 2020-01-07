namespace Pentagon.Utilities.Console.Demo {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensions.Console.Cli;

    [CliCommand("hierarchy", AlternedNames = new []{"h", "hier"}, Description = "Show command hieararchy.")]
    class BuildCliCommand 
    {
        class Handler : CliCommandHandler<BuildCliCommand>
        {
            /// <inheritdoc />
            protected override Task<int> ExecuteAsync(BuildCliCommand command, CancellationToken cancellationToken)
            {
                var commandHierarchy = CliCommandContext.Instance.CommandHierarchy;

                Console.WriteLine(commandHierarchy.ToTreeString());

                return Task.FromResult(0);
            }
        }
    }
}