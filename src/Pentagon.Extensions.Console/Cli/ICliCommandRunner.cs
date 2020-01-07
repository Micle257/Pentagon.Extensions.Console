// -----------------------------------------------------------------------
//  <copyright file="ICliCommandRunner.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICliCommandRunner
    {
        Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default);

        Task<int> RunAsync(string cli, CancellationToken cancellationToken = default);
    }
}