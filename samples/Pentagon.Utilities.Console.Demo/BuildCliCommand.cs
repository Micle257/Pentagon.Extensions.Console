namespace Pentagon.Utilities.Console.Demo {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensions.Console.Cli;

    [CliCommand("hierarchy", AlternedNames = new []{"h", "hier"}, Description = "Show command hieararchy.")]
    class BuildCliCommand : Root
    {
        class Handler : ICliCommandHandler<BuildCliCommand>
        {
            /// <inheritdoc />
            public Task<int> ExecuteAsync(BuildCliCommand command, CancellationToken cancellationToken)
            {
                var commandHierarchy = CliCommandCompileContext.Instance.CommandHierarchy;

                Console.WriteLine(commandHierarchy.ToTreeString());

                return Task.FromResult(0);
            }
        }
    }
}