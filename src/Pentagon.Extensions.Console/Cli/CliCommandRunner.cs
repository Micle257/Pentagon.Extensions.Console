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
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class CliCommandRunner : ICliCommandRunner
    {
        readonly IServiceScopeFactory _scopeFactory;

        ILogger<CliCommandRunner> _logger;

        [NotNull]
        readonly CliOptions _options;

        public CliCommandRunner(IServiceScopeFactory scopeFactory,
                                IOptions<CliOptions> options)
        {
            _scopeFactory = scopeFactory;
            _logger = scopeFactory.CreateScope().ServiceProvider.GetService<ILogger<CliCommandRunner>>();
            _options = options?.Value ?? new CliOptions();
        }

        public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
        {
            if (!_options.UseAnnotatedCommands)
                CliCommandCompileContext.Instance.DisableAnnotatedCommand();

            InitializeHandlers(CliCommandCompileContext.Instance.CommandInfos);

            var root = CliCommandCompileContext.Instance.RootCommandInfo;

            var result = await root.Command.InvokeAsync(args).ConfigureAwait(false);

            return result;
        }

        /// <inheritdoc />
        public async Task<int> RunAsync(string cli, CancellationToken cancellationToken = default)
        {
            if (!_options.UseAnnotatedCommands)
                CliCommandCompileContext.Instance.DisableAnnotatedCommand();

            InitializeHandlers(CliCommandCompileContext.Instance.CommandInfos);

            var root = CliCommandCompileContext.Instance.RootCommandInfo;

            var result = await root.Command.InvokeAsync(cli).ConfigureAwait(false);

            return result;
        }

        public static IEnumerable<object> Parse(string[] args)
        {
            var root = CliCommandCompileContext.Instance.RootCommandInfo;

            var parseResult = root.Command.Parse(args);

            return GetAllCommands(parseResult);
        }

        public static IEnumerable<object> GetAllCommands(ParseResult parseResult)
        {
            var infos = CliCommandCompileContext.Instance.CommandInfos;

            foreach (var node in infos)
            {
                var commandResult = parseResult.FindResultFor(node.Command);

                if (commandResult != null)
                {
                    if (node.Describer.Type == null)
                        continue;

                    var command = Activator.CreateInstance(node.Describer.Type);

                    foreach (var cliOptionInfo in node.Options)
                    {
                        var optionResult = parseResult.FindResultFor(cliOptionInfo.Option);

                        if (optionResult != null)
                        {
                            var value = parseResult.ValueForOption(cliOptionInfo.Option.RawAliases[0]);

                            SetValue(cliOptionInfo.Describer.PropertyInfo, command, value);
                        }
                    }

                    foreach (var cliArgumentInfo in node.Arguments)
                    {
                        var argumentResult = parseResult.FindResultFor(cliArgumentInfo.Argument);

                        if (argumentResult != null)
                        {
                            var values = argumentResult.GetValueOrDefault();

                            SetValue(cliArgumentInfo.Describer.PropertyInfo, command, values);
                        }
                    }

                    yield return command;
                }
            }

            void SetValue(MemberInfo cliOptionInfo, object command, object value)
            {
                try
                {
                    var valueStr = value.ToString();

                    if (!string.IsNullOrWhiteSpace(valueStr))
                    {
                        if (valueStr.ToLowerInvariant().IsAnyEqual("false", "f", "0"))
                            value = false;
                        else if (valueStr.ToLowerInvariant().IsAnyEqual("true", "t", "1"))
                            value = true;
                    }

                    if (cliOptionInfo is FieldInfo field)
                        field.SetValue(command, value);
                    else if (cliOptionInfo is PropertyInfo prop)
                        prop.SetValue(command, value);
                    else
                        throw new ArgumentException($"Invalid member: {cliOptionInfo}");
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        void InitializeHandlers(IReadOnlyList<CliCommandInfo> commandInfos)
        {
            using var scope = _scopeFactory.CreateScope();

            foreach (var cliCommandInfo in commandInfos)
            {
                var commandHandler = scope.ServiceProvider.GetService<ICommandHandler>();

                if (commandHandler == null)
                {
                    _logger?.LogTrace("Command handler for {CommandType} not found.", cliCommandInfo.Describer.Type);
                }
                else
                {
                    _logger?.LogTrace("Command handler for {CommandType} found.", cliCommandInfo.Describer.Type);

                    ((Command)cliCommandInfo.Command).Handler = commandHandler;
                }
            }
        }
    }
}