// -----------------------------------------------------------------------
//  <copyright file="Program.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Demo
{
    using System;
    using System.CommandLine.Invocation;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Extensions.Console;
    using Extensions.Console.Cli;
    using Extensions.Console.Controls;
    using Microsoft.Extensions.DependencyInjection;

    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection()
                   .AddCli(c => c.InvokeAllMatchedHandlers = true);

            var di = services.BuildServiceProvider();

            var runner = di.GetRequiredService<ICliCommandRunner>();

            while (true)
            {
                Console.Write("CLI: ");
                var cli = Console.ReadLine();

                var r = await runner.RunAsync(cli).ConfigureAwait(false);

                Console.WriteLine();
                Console.WriteLine($"Code: {r}");
                Console.ReadKey();
                Console.Clear();
            }
        }
    }
}