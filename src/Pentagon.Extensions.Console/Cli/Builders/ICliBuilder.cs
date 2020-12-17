namespace Pentagon.Extensions.Console.Cli.Builders {
    using System;
    using JetBrains.Annotations;

    public interface ICliBuilder
    {
        [NotNull]
        ICliCommandBuilder<T> HasCommand<T>();

        [NotNull]
        ICliCommandBuilder HasCommand([NotNull] Type type);

        // [NotNull]
        // ICliBuilder HasImplicitRoot();
    }
}