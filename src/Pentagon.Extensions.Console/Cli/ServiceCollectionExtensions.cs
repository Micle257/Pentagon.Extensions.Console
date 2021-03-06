// -----------------------------------------------------------------------
//  <copyright file="ServiceCollectionExtensions.cs">
//   Copyright (c) Michal Pokorn�. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System;
    using System.CommandLine.Invocation;
    using System.Linq;
    using Builders;
    using FluentValidation;
    using Helpers;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class ServiceCollectionExtensions
    {
        [NotNull]
        public static IServiceCollection AddCli([NotNull] this IServiceCollection services,
                                                        [NotNull] Action<ICliBuilder> builderConfigure
                                                      , Action<CliOptions> configure = null)
        {
            var builder = new CliBuilder();

            builderConfigure(builder);

            var cli = builder.Build();

            CliCommandCompileContext.Instance.AddDescribers(cli);

            return services.AddCli(configure);
        }

        [NotNull]
        public static IServiceCollection AddCli([NotNull] this IServiceCollection services, Action<CliOptions> configure = null)
        {
            services.AddCliCommandRunner()
                    .AddCliCommandHandlers()
                    .AddCliCommandValidators()
                    .AddInvocationCommandHandler<InvocationCommandHandler>();

            services.Configure(configure ?? (options => { }));

            return services;
        }

        [NotNull]
        public static IServiceCollection AddCommandInvokeService<T>([NotNull] this IServiceCollection services)
                where T : class, ICommandInvokeService
        {
            services.AddTransient<ICommandInvokeService, T>();

            return services;
        }

        [NotNull]
        public static IServiceCollection AddCliCommandRunner([NotNull] this IServiceCollection services)
        {
            services.AddTransient<ICliCommandRunner, CliCommandRunner>();

            return services;
        }

        [NotNull]
        public static IServiceCollection AddCliCommandHandlers([NotNull] this IServiceCollection services)
        {
            var commands = AppDomain.CurrentDomain
                                    .GetLoadedTypes()
                                    .Where(a => a.IsClass && !a.IsAbstract)
                                    .Distinct();

            foreach (var command in commands)
            {
                var interfaces = command.GetInterfaces()
                                        .Where(b => b.GenericTypeArguments.Length == 1)
                                        .FirstOrDefault(a => a.GetGenericTypeDefinition() == typeof(ICliCommandHandler<>) || a.GetGenericTypeDefinition() == typeof(ICliCommandPropertyHandler<>));

                if (interfaces == null)
                    continue;

                services.Add(ServiceDescriptor.Scoped(interfaces, command));
            }

            return services;
        }

        [NotNull]
        public static IServiceCollection AddCliCommandValidators([NotNull] this IServiceCollection services)
        {
            var commandTypes = CliCommandCompileContext.Instance.CommandDescribers.Select(a => a.Type).ToList();

            var types = AppDomain.CurrentDomain
                                    .GetLoadedTypes()
                                    .Where(a => a.IsClass && !a.IsAbstract)
                                    .Distinct();

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces()
                                        .Where(b => b.GenericTypeArguments.Length == 1)
                                        .FirstOrDefault(a => a.GetGenericTypeDefinition() == typeof(IValidator<>));

                if (interfaces == null)
                    continue;

                if (!commandTypes.Contains(interfaces.GenericTypeArguments[0]))
                    continue;

                services.AddTransient(interfaces, type);
            }

            return services;
        }

        [NotNull]
        public static IServiceCollection AddInvocationCommandHandler<T>([NotNull] this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where T : ICommandHandler
        {
            services.Replace(ServiceDescriptor.Describe(typeof(ICommandHandler), typeof(T), serviceLifetime));

            return services;
        }
    }
}