namespace Pentagon.Extensions.Console.Cli {
    using System;
    using System.Linq;
    using Helpers;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        [NotNull]
        public static IServiceCollection AddCli([NotNull] this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            services.AddCliCommandRunner()
                    .AddCliCommandHandlers(serviceLifetime);

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

                services.Add(ServiceDescriptor.Describe(serviceType: interfaces, implementationType: command, lifetime: serviceLifetime));
            }

            return services;
        }
    }
}