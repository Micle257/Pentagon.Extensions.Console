// -----------------------------------------------------------------------
//  <copyright file="ProcessExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Threading;

    public static class ProcessExtensions
    {
        public static Task<int> StartAndWaitAsync(this Process process, CancellationToken cancellationToken) => StartAndWaitCoreAsync(process).WithCancellation(cancellationToken);

        static Task<int> StartAndWaitCoreAsync(this Process process)
        {
            var source = new TaskCompletionSource<int>();

            process.Exited += (_, __) => { source.SetResult(process.ExitCode); };

            process.Start();

            return source.Task;
        }
    }
}