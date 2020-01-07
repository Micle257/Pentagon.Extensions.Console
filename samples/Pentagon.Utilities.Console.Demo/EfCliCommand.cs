namespace Pentagon.Utilities.Console.Demo {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensions.Console.Cli;

    [CliCommand("ef", AlternedNames = new []{ "entity"})]
    class EfCliCommand
    {
        [CliOption()]
        public bool DryRun { get; set; }

        [CliArgument(MaximumNumberOfValues = 3)]
        public IEnumerable<string> Force { get; set; }

        class Handler : ICliCommandHandler<EfCliCommand>
        {
            /// <inheritdoc />
            public Task<int> ExecuteAsync(EfCliCommand command, CancellationToken cancellationToken)
            {
                return Task.FromResult(0);
            }
        }
    }
}