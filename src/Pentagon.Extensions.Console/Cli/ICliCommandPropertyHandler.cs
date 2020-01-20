// -----------------------------------------------------------------------
//  <copyright file="ICliCommandPropertyHandler.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    [PublicAPI]
    public interface ICliCommandPropertyHandler<out TCommand>
    {
        [NotNull]
        TCommand Command { get; }

        [NotNull]
        Task<int> ExecuteAsync(CancellationToken cancellationToken);
    }
}