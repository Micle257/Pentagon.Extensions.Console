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
    using System.Threading;
    using System.Threading.Tasks;
    using Extensions.Console;
    using Extensions.Console.Cli;
    using Extensions.Console.Controls;
    using Microsoft.Extensions.DependencyInjection;

    class RandomRoot
    {

    }

    class RandomCommand
    {
        public bool Boom { get; set; }

        public string Data { get; set; }

        [CliOption]
        public string W { get; set; }

        class Handler : ICliCommandHandler<RandomCommand>
        {
            /// <inheritdoc />
            public Task<int> ExecuteAsync(RandomCommand command, CancellationToken cancellationToken)
            {
                Console.WriteLine(command.Data);

                return Task.FromResult(0);
            }
        }
    }

    class Random2Command
    {
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            //var services = new ServiceCollection()
            //              .AddCli(b =>
            //                      {
            //                          // TODO this should be implicit
            //                          b.HasCommand<RandomRoot>()
            //                           .IsRoot();
            //
            //                          b.HasCommand<RandomCommand>()
            //                           .IsSubCommandFor<RandomRoot>()
            //                           .HasSubCommand<Random2Command>()
            //                           .WithName("r")
            //                           .WithName("rand")
            //                           .WithDescription("what")
            //                           .HasArgument(c => c.Data, c => c.IsRequired = true)
            //                           .HasOption(c => c.Boom);
            //
            //                          // TODO this should be implicit
            //                          b.HasCommand<Random2Command>();
            //                      },
            //                      c =>
            //                      {
            //                          c.InvokeAllMatchedHandlers = true;
            //                          c.UseAnnotatedCommands = false;
                                  //});

            var services = new ServiceCollection()
                   .AddCli(b =>
                           {
                               b.HasCommand<RandomCommand>()
                                .IsSubCommandFor<EfCliCommand>()
                                .WithName("r")
                                .WithName("rand")
                                .WithDescription("what")
                                .HasArgument(c => c.Data, c => c.IsRequired = true)
                                .HasOption(c => c.Boom);
                           },
                           c => c.InvokeAllMatchedHandlers = true);

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