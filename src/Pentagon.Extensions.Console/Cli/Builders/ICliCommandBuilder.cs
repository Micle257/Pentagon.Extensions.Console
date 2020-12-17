namespace Pentagon.Extensions.Console.Cli.Builders {
    using System;
    using JetBrains.Annotations;

    public interface ICliCommandBuilder
    {
        [NotNull]
        Type Type { get; }

        [NotNull]
        ICliCommandBuilder HasOption([NotNull] string memberName, Action<CliOptionAttribute> configure = null);

        [NotNull]
        ICliCommandBuilder HasArgument([NotNull] string memberName, Action<CliArgumentAttribute> configure = null);

        [NotNull]
        ICliCommandBuilder WithName([NotNull] string name);

        [NotNull]
        ICliCommandBuilder WithDescription([NotNull] string description);

        [NotNull]
        ICliCommandBuilder IsRoot();

        [NotNull]
        ICliCommandBuilder HasSubCommand([NotNull] Type subType);

        [NotNull]
        ICliCommandBuilder IsSubCommandFor([NotNull] Type subType);

        [NotNull]
        CliCommandDescriber Build();
    }
}