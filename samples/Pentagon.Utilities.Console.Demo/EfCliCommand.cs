namespace Pentagon.Utilities.Console.Demo {
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensions.Console.Cli;
    using FluentValidation;

    [CliRootCommand]
    [SubCliCommand(typeof(EfCliCommand))]
    class Root
    {

    }

    [CliCommand("ef", AlternedNames = new []{ "entity"})]
    class EfCliCommand
    {
        [CliOption()]
        public bool DryRun { get; set; }

        [CliOption()]
        public bool NotDryRun { get; set; }

        [CliArgument(IsRequired = false)]
        public string Force { get; set; }

        class Handler : ICliCommandPropertyHandler<EfCliCommand>, ICliCommandHandler<EfCliCommand>
        {
            /// <inheritdoc />
            public async Task<int> ExecuteAsync( CancellationToken cancellationToken)
            {
                await Task.Delay(5000, cancellationToken).ConfigureAwait(false);

                return await Task.FromResult(1000).ConfigureAwait(false);
            }

            /// <inheritdoc />
            public EfCliCommand Command { get;  }

            /// <inheritdoc />
            public Task<int> ExecuteAsync(EfCliCommand command, CancellationToken cancellationToken)
            {
                return ExecuteAsync(cancellationToken);
            }
        }
    }
}