// -----------------------------------------------------------------------
//  <copyright file="VoidCliCommandHandler.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System;
    using System.CommandLine.Invocation;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    // TODO log
    class InvocationCommandHandler : ICommandHandler
    {
        readonly ILogger<InvocationCommandHandler> _logger;
        readonly IServiceScopeFactory _scopeFactory;
        readonly CliOptions _options;

        public InvocationCommandHandler(ILogger<InvocationCommandHandler> logger,
                                        IServiceScopeFactory scopeFactory,
                                        IOptions<CliOptions> options)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _options = options?.Value ?? new CliOptions();
        }

        /// <inheritdoc />
        public async Task<int> InvokeAsync(InvocationContext context)
        {
            using var scope = _scopeFactory.CreateScope();

            var allCommands = CliCommandRunner.GetAllCommands(context.ParseResult).Reverse().ToList();
            
            int? result = null;

            foreach (var command in allCommands)
            {
                var handler = scope.ServiceProvider.GetService(typeof(ICliCommandHandler<>).MakeGenericType(command.GetType()));

                // if handler not in DI, continue
                if (handler == null)
                    continue;

                Console.WriteLine($"Invoking {command.GetType().Name}");

                var method = handler.GetType().GetMethod(nameof(ICliCommandHandler<object>.ExecuteAsync), new[] { command.GetType(), typeof(CancellationToken) });

                // TODO investigate if cancellation token works with IHosting
                var taskOfInt = (Task<int>)method.Invoke(handler, new[] { command, context.GetCancellationToken() });

                try
                {
                    result = await taskOfInt.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    await OnCancelAsync().ConfigureAwait(false);
                    result = StatusCodes.Cancel;
                }
                catch (Exception e)
                {
                    await OnErrorAsync(e).ConfigureAwait(false);
                    result = StatusCodes.Error;
                }

                if (!_options.InvokeAllMatchedHandlers)
                    break;

                if (result == StatusCodes.Cancel)
                    break;

                if (_options.ExitOnError && result == StatusCodes.Error)
                    break;
            }

            return result.GetValueOrDefault();
        }

        protected virtual Task OnCancelAsync()
        {
            _logger?.LogDebug("Command was cancelled: {TypeName}.", GetType().Name);

            return Task.CompletedTask;
        }

        protected virtual Task OnErrorAsync(Exception e)
        {
            _logger?.LogError(e, "Command execution failed: {TypeName}. {ExceptionMessage}", GetType().Name, e.Message);

            return Task.CompletedTask;
        }
    }
}