// -----------------------------------------------------------------------
//  <copyright file="ICliCommandHandler.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System.CommandLine.Invocation;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public interface ICliCommandHandler<in TCommand>
    {
        [NotNull]
        Task<int> ExecuteAsync(TCommand command, CancellationToken cancellationToken = default);
    }
}