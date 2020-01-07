namespace Pentagon.Extensions.Console.Cli {
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public interface ICliCommandHandler<TCommand>
    {
        [NotNull]
        Task<int> ExecuteAsync(TCommand command, CancellationToken cancellationToken = default);
    }
}