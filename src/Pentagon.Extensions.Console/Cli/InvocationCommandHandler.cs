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
    using FluentValidation;
    using FluentValidation.Results;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    class InvocationCommandHandler : ICommandHandler
    {
        readonly IServiceScopeFactory _scopeFactory;
        readonly CliOptions _options;
        readonly ILogger<InvocationCommandHandler> _logger;

        public InvocationCommandHandler(IServiceScopeFactory scopeFactory,
                                        IOptions<CliOptions> options)
        {
            _scopeFactory = scopeFactory;
            _options = options?.Value ?? new CliOptions();

            _logger = _scopeFactory.CreateScope().ServiceProvider.GetService<ILogger<InvocationCommandHandler>>();
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

                _logger?.LogInformation("Invoking command handler: {Handler}", command.GetType().Name);

                var method = handler.GetType().GetMethod(nameof(ICliCommandHandler<object>.ExecuteAsync), new[] { command.GetType(), typeof(CancellationToken) });

                // TODO investigate if cancellation token works with IHosting
                var taskOfInt = (Task<int>)method.Invoke(handler, new[] { command, context.GetCancellationToken() });

                var validation = Validate(command, scope.ServiceProvider);

                if (!validation.IsValid)
                {
                    await OnValidationErrorAsync(new ArgumentException(validation.ToString())).ConfigureAwait(false);
                    result = StatusCodes.Error;
                }
                else
                {
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
                }

                if (!_options.InvokeAllMatchedHandlers)
                    break;

                if (result == StatusCodes.Cancel)
                    break;

                if (_options.ExitOnError && result == StatusCodes.Error)
                    break;
            }

            context.ResultCode = result.GetValueOrDefault();

            return context.ResultCode;
        }

        ValidationResult Validate(object command, IServiceProvider di)
        {
            var validator = (IValidator)di.GetService(typeof(IValidator<>).MakeGenericType(command.GetType()));

            if (validator == null)
                return new ValidationResult();

            var validationResult = validator.Validate(command);

            return validationResult;
        }

        protected virtual Task OnCancelAsync()
        {
            ConsoleWriter.WriteError("Command was cancelled");

            _logger?.LogDebug("Command was cancelled: {TypeName}.", GetType().Name);

            return Task.CompletedTask;
        }

        protected virtual Task OnValidationErrorAsync(Exception e)
        {
            ConsoleWriter.WriteError($"Command is not valid: {e.Message}");

            _logger?.LogError(e, "Command is not valid: {TypeName}. {ExceptionMessage}", GetType().Name, e.Message);

            return Task.CompletedTask;
        }

        protected virtual Task OnErrorAsync(Exception e)
        {
            ConsoleWriter.WriteError($"Command execution failed: {e.Message}");

            _logger?.LogError(e, "Command execution failed: {TypeName}. {ExceptionMessage}", GetType().Name, e.Message);

            return Task.CompletedTask;
        }
    }
}