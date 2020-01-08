namespace Pentagon.Utilities.Console.Demo {
    using System.Collections.Generic;
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

        [CliArgument(MaximumNumberOfValues = 3)]
        public IEnumerable<string> Force { get; set; }

        class Handler : ICliCommandHandler<EfCliCommand>
        {
            /// <inheritdoc />
            public async Task<int> ExecuteAsync(EfCliCommand command, CancellationToken cancellationToken)
            {
                await Task.Delay(5000, cancellationToken).ConfigureAwait(false);

                return await Task.FromResult(1000).ConfigureAwait(false);
            }
        }

        class Validator : AbstractValidator<EfCliCommand>
        {
            public Validator()
            {
                RuleFor(command => command)
                       .Must(a => a.DryRun != a.NotDryRun)
                       .WithMessage("Only one option must be specified for dry run.");
            }
        }
    }
}