// -----------------------------------------------------------------------
//  <copyright file="Program.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Demo
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensions.Console;
    using Extensions.Console.Cli;
    using Extensions.Console.Controls;
    using Microsoft.Extensions.DependencyInjection;

    [CliCommand("ef", AlternedNames = new []{ "entity"})]
    class EfCliCommand 
    {

    }
    [CliCommand("hierarchy", AlternedNames = new []{"h", "hier"}, Description = "Show command hieararchy.")]
    class BuildCliCommand 
    {
        class Handler : CliCommandHandler<BuildCliCommand>
        {
            /// <inheritdoc />
            protected override Task<int> ExecuteAsync(BuildCliCommand command, CancellationToken cancellationToken)
            {
                var commandHierarchy = CliCommandContext.GetCommandHierarchy();

                Console.WriteLine(commandHierarchy.ToTreeString());

                return Task.FromResult(0);
            }
        }
    }

    [CliCommand("migrations")]
    class MigrationsCliCommand : EfCliCommand
    {
    }

    [CliCommand("database")]
    class DatabaseCliCommand : EfCliCommand
    {
    }

    [CliCommand("drop")]
    class DropDatabaseCliCommand : DatabaseCliCommand
    {
    }

    [CliCommand("update")]
    class UpdateDatabaseCliCommand : DatabaseCliCommand
    {
        [CliOption()]
        public bool DryRun { get; set; }

        [CliArgument()]
        public string Force { get; set; }

        class Handler : CliCommandHandler<UpdateDatabaseCliCommand>
        {
            /// <inheritdoc />
            protected override Task<int> ExecuteAsync(UpdateDatabaseCliCommand command, CancellationToken cancellationToken)
            {
                return Task.FromResult(0);
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection().AddCli();

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

            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("cs");

            var combo = new ComboBoxCliControl<string>("Teststets asd asd asd asd sad asd sad sad sad gfgh fg fg fg fg fg fg fg fg f gfg f gfg fg f gf?",
                                                       new[]
                                                       {
                                                               "LOl", "no", "piss",
                                                               "Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd ",
                                                               "no"
                                                       });

            var comboFields = combo.Run();
        }
    }
}