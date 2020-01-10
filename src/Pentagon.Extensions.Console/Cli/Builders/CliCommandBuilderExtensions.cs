namespace Pentagon.Extensions.Console.Cli.Builders {
    using System;
    using System.Linq.Expressions;
    using JetBrains.Annotations;

    public static class CliCommandBuilderExtensions
    {
        [NotNull]
        public static ICliCommandBuilder<TCommand> HasOption<TCommand, T>([NotNull] this ICliCommandBuilder<TCommand> builder,
                                                                          [NotNull] Expression<Func<TCommand, T>> property,
                                                                          [NotNull] params string[] aliases)
        {
            return builder.HasOption(property,
                                     c => { c.Aliases = aliases; });
        }
    }
}