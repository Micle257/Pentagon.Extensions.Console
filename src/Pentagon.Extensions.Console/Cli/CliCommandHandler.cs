// -----------------------------------------------------------------------
//  <copyright file="CliCommandToOptions.cs">
//   Copyright (c) Smartdata s.r.o. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System.CommandLine.Invocation;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public abstract class CliCommandHandler<T> : ICommandHandler, ICliCommandHandler<T>
    {
        /// <inheritdoc />
        public Task<int> InvokeAsync(InvocationContext context)
        {
            var command = context.ParseResult.GetCommand<T>();

            return ((ICliCommandHandler<T>) this).ExecuteAsync(command, context.GetCancellationToken());
        }

        /// <inheritdoc />
        Task<int> ICliCommandHandler<T>.ExecuteAsync(T command, CancellationToken cancellationToken) => ExecuteAsync(command, cancellationToken);

        protected abstract Task<int> ExecuteAsync([NotNull] T command, CancellationToken cancellationToken);
    }
}