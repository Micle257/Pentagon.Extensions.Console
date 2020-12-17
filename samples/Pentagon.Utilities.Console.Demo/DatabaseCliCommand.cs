namespace Pentagon.Utilities.Console.Demo {
    using System.Threading;
    using System.Threading.Tasks;
    using Extensions.Console.Cli;

    [CliCommand("database")]
    class DatabaseCliCommand : EfCliCommand
    {

        class Handler : ICliCommandHandler<DatabaseCliCommand>
        {
            /// <inheritdoc />
            public Task<int> ExecuteAsync(DatabaseCliCommand command, CancellationToken cancellationToken)
            {
                return Task.FromResult(0);
            }
        }
    }
}