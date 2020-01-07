// -----------------------------------------------------------------------
//  <copyright file="ServiceCollectionExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System;
    using System.CommandLine.Invocation;
    using System.Linq;
    using Helpers;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class ServiceCollectionExtensions
    {
        [NotNull]
        public static IServiceCollection AddCli([NotNull] this IServiceCollection services, Action<CliOptions> configure = null, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            services.AddCliCommandRunner()
                    .AddCliCommandHandlers(serviceLifetime)
                    .AddInvocationCommandHandler<InvocationCommandHandler>();

            services.Configure(configure ?? (options => {}));

            return services;
        }

        [NotNull]
        public static IServiceCollection AddCliCommandRunner([NotNull] this IServiceCollection services)
        {
            services.AddTransient<ICliCommandRunner, CliCommandRunner>();

            return services;
        }

        [NotNull]
        public static IServiceCollection AddCliCommandHandlers([NotNull] this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var commands = AppDomain.CurrentDomain
                                    .GetLoadedTypes()
                                    .Where(a => a.IsClass && !a.IsAbstract)
                                    .Distinct();

            foreach (var command in commands)
            {
                var interfaces = command.GetInterfaces()
                                        .Where(b => b.GenericTypeArguments.Length == 1)
                                        .FirstOrDefault(a => a.GetGenericTypeDefinition() == typeof(ICliCommandHandler<>));

                if (interfaces == null)
                    continue;

                services.Add(ServiceDescriptor.Describe(interfaces, command, serviceLifetime));
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