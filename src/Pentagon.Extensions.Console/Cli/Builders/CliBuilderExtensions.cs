namespace Pentagon.Extensions.Console.Cli.Builders {
    using JetBrains.Annotations;

    public static class CliBuilderExtensions
    {
        [NotNull]
        public static ICliCommandBuilder<T> HasCommand<T>(this ICliBuilder builder, [NotNull] string name) =>
                builder.HasCommand<T>()
                       .WithName(name);
    }
}