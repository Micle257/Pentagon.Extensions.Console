// -----------------------------------------------------------------------
//  <copyright file="CliCommandRunner.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    // TODO log
    public class CliCommandRunner : ICliCommandRunner
    {
        readonly IServiceScopeFactory _scopeFactory;
        readonly CliOptions _options;

        public CliCommandRunner(IServiceScopeFactory scopeFactory,
                                IOptions<CliOptions> options)
        {
            _scopeFactory = scopeFactory;
            _options = options?.Value ?? new CliOptions();
        }

        public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
        {
            InitializeHandlers(CliCommandContext.Instance.CommandInfos);

            var root = CliCommandContext.Instance.RootCommandInfo;

            var result = await root.Command.InvokeAsync(args).ConfigureAwait(false);

            return result;
        }

        /// <inheritdoc />
        public async Task<int> RunAsync(string cli, CancellationToken cancellationToken = default)
        {
            InitializeHandlers(CliCommandContext.Instance.CommandInfos);

            var root = CliCommandContext.Instance.RootCommandInfo;

            var result = await root.Command.InvokeAsync(cli).ConfigureAwait(false);

            return result;
        }

        public static IEnumerable<object> Parse(string[] args)
        {
            var root = CliCommandContext.Instance.RootCommandInfo;

            var parseResult = root.Command.Parse(args);

            return GetAllCommands(parseResult);
        }

        public static IEnumerable<object> GetAllCommands(ParseResult parseResult)
        {
            var infos = CliCommandContext.Instance.CommandInfos;

            foreach (var node in infos)
            {
                var commandResult = parseResult.FindResultFor(node.Command);

                if (commandResult != null)
                {
                    var command = Activator.CreateInstance(node.Describer.Type);

                    foreach (var cliOptionInfo in node.Options)
                    {
                        var optionResult = parseResult.FindResultFor(cliOptionInfo.Option);

                        if (optionResult != null)
                        {
                            var value = parseResult.ValueForOption(cliOptionInfo.Option.RawAliases[0]);

                            cliOptionInfo.Describer.PropertyInfo.SetValue(command, value);
                        }
                    }

                    foreach (var cliArgumentInfo in node.Arguments)
                    {
                        var argumentResult = parseResult.FindResultFor(cliArgumentInfo.Argument);

                        if (argumentResult != null)
                        {
                            var values = argumentResult.GetValueOrDefault();

                            cliArgumentInfo.Describer.PropertyInfo.SetValue(command, values);
                        }
                    }

                    yield return command;
                }
            }
        }

        void InitializeHandlers(IReadOnlyList<CliCommandInfo> commandInfos)
        {
            using var scope = _scopeFactory.CreateScope();

            foreach (var cliCommandInfo in commandInfos)
            {
                ((Command)cliCommandInfo.Command).Handler = scope.ServiceProvider.GetService<ICommandHandler>();
            }
        }
    }
}