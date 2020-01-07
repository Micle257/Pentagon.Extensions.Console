namespace Pentagon.Extensions.Console.Cli
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;

    public class CliCommandRunner : ICliCommandRunner
    {
        readonly IServiceScopeFactory _scopeFactory;

        public CliCommandRunner(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
        {
            InitializeHandlers(CliCommandContext.CommandInfos);

            var root = CliCommandContext.GetRootCommand();

            var result = await root.Command.InvokeAsync(args).ConfigureAwait(false);

            return result;
        }

        /// <inheritdoc />
        public async Task<int> RunAsync(string cli, CancellationToken cancellationToken = default)
        {
            InitializeHandlers(CliCommandContext.CommandInfos);

            var root = CliCommandContext.GetRootCommand();

            var result = await root.Command.InvokeAsync(cli).ConfigureAwait(false);

            return result;
        }

        void InitializeHandlers(IReadOnlyList<CliCommandInfo> commandInfos)
        {
            using var scope = _scopeFactory.CreateScope();

            foreach (var cliCommandInfo in commandInfos)
            {
                ((Command)cliCommandInfo.Command).Handler = scope.ServiceProvider.GetService(typeof(ICliCommandHandler<>).MakeGenericType(cliCommandInfo.Describer.Type)) as ICommandHandler;
            }
        }

        static IEnumerable<object> GetAllCommands(ParseResult parseResult)
        {
            var infos = CliCommandContext.CommandInfos;

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

        public static IEnumerable<object> Parse(string[] args)
        {
            var root = CliCommandContext.GetRootCommand();

            var parseResult = root.Command.Parse(args);

            return GetAllCommands(parseResult);
        }
    }
}