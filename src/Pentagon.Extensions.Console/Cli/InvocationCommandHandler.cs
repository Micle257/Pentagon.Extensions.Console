// -----------------------------------------------------------------------
//  <copyright file="VoidCliCommandHandler.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine.Invocation;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using FluentValidation;
    using FluentValidation.Results;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    class InvocationCommandHandler : ICommandHandler
    {
        readonly IServiceScopeFactory _scopeFactory;

        [NotNull]
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

            var allCommands = CliCommandRunner.GetAllCommands(context.ParseResult).ToList();

            int? result = null;

            var cancellationToken = context.GetCancellationToken();

            cancellationToken.Register(() => { _logger?.LogDebug("InvocationContext triggered cancellation token."); });

            foreach (var command in allCommands)
            {
                var handler = scope.ServiceProvider.GetService(typeof(ICliCommandHandler<>).MakeGenericType(command.GetType()));

                // if handler not in DI, continue
                if (handler == null)
                {
                    handler = scope.ServiceProvider.GetService(typeof(ICliCommandPropertyHandler<>).MakeGenericType(command.GetType()));

                    if (handler == null)
                        continue;
                }

                foreach (var invokeService in scope.ServiceProvider.GetServices<ICommandInvokeService>())
                {
                    await invokeService.ProcessAsync(command).ConfigureAwait(false);
                }

                _logger?.LogInformation("Invoking command handler: {Handler}", command.GetType().Name);

                var method = handler.GetType().GetMethod(nameof(ICliCommandHandler<object>.ExecuteAsync), new[] { command.GetType(), typeof(CancellationToken) });

                var isMethodWithoutCommandParameter = false;

                if (method == null)
                {
                    method = handler.GetType().GetMethod(nameof(ICliCommandPropertyHandler<object>.ExecuteAsync), new[] { typeof(CancellationToken) });

                    if (method == null)
                    {
                        _logger?.LogWarning("Method for executing command {CommandType} was not found.", command.GetType());
                        continue;
                    }

                    isMethodWithoutCommandParameter = true;
                }

                // if handler implements ICliCommandPropertyHandler...
                if (typeof(ICliCommandPropertyHandler<>).MakeGenericType(command.GetType()).IsAssignableFrom(handler.GetType()))
                {
                    var propertyInfo = handler.GetType().GetProperty(nameof(ICliCommandPropertyHandler<object>.Command));

                    if (propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(handler, command);
                    }
                    else
                    {
                        var back = handler.GetType().GetField($"<{nameof(ICliCommandPropertyHandler<object>.Command)}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

                        if (back == null)
                            throw new InvalidOperationException($"Property '{typeof(ICliCommandPropertyHandler<>).Name}.Command' must have at least private setter.");

                        back.SetValue(handler, command);
                    }
                }

                var validation = Validate(command, scope.ServiceProvider);

                if (!validation.IsValid)
                {
                    _logger?.LogDebug("CLI command validation failed for {CommandType}, reason: {Reason}", command.GetType(), validation.ToString());

                    ConsoleWriter.WriteError($"Command is not valid: {validation.ToString()}");
                    Console.WriteLine();

                    result = StatusCodes.Error;
                }
                else
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        _logger?.LogDebug("Command: {@Command}", command);

                        var taskOfInt = isMethodWithoutCommandParameter ? (Task<int>)method.Invoke(handler, new object[] { cancellationToken }) : (Task<int>)method.Invoke(handler, new[] { command, cancellationToken });

                        result = await taskOfInt.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        ConsoleWriter.WriteError("Command was cancelled");
                        Console.WriteLine();

                        _logger?.LogInformation("Command was cancelled: {TypeName}.", GetType().Name);

                        result = StatusCodes.Cancel;
                    }
                    catch (Exception e)
                    {
                        ConsoleWriter.WriteError($"Command execution failed: {e.Message}");
                        Console.WriteLine();

                        _logger?.LogError(e, "Command execution failed: {TypeName}. {ExceptionMessage}", GetType().Name, e.Message);

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
            {
                _logger?.LogTrace("Validator not found for {CommandType}", command.GetType());
                return new ValidationResult();
            }

            _logger?.LogTrace("Validator found for {CommandType}", command.GetType());

            var validationResult = validator.Validate(command);

            return validationResult;
        }
    }
}