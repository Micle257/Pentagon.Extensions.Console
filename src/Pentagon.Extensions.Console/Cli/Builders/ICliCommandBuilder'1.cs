namespace Pentagon.Extensions.Console.Cli.Builders {
    using System;
    using System.Linq.Expressions;
    using JetBrains.Annotations;

    public interface ICliCommandBuilder<TCommand> : ICliCommandBuilder
    {
        [NotNull]
        ICliCommandBuilder<TCommand> HasOption<T>([NotNull] Expression<Func<TCommand, T>> property, Action<CliOptionAttribute> configure = null);

        [NotNull]
        ICliCommandBuilder<TCommand> HasArgument<T>([NotNull] Expression<Func<TCommand, T>> property, Action<CliArgumentAttribute> configure = null);

        [NotNull]
        new ICliCommandBuilder<TCommand> WithName([NotNull] string name);

        [NotNull]
        new ICliCommandBuilder<TCommand> WithDescription([NotNull] string description);

        [NotNull]
        new ICliCommandBuilder<TCommand> IsRoot();

        [NotNull]
        ICliCommandBuilder<TCommand> HasSubCommand<T>();

        [NotNull]
        ICliCommandBuilder<TCommand> IsSubCommandFor<TSub>();
    }
}