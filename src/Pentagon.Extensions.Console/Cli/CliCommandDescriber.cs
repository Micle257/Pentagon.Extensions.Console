namespace Pentagon.Extensions.Console.Cli {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CliCommandDescriber
    {
        public Type Type { get; }

        public CliCommandAttribute Attribute { get; }

        public IReadOnlyCollection<CliOptionDescriber> Options { get; }

        public IReadOnlyCollection<CliArgumentDescriber> Arguments { get; }

        public CliCommandDescriber(Type type, CliCommandAttribute attribute, IEnumerable<CliOptionDescriber> options, IEnumerable<CliArgumentDescriber> arguments)
        {
            Type      = type;
            Attribute = attribute;
            Options   = options.ToList().AsReadOnly();
            Arguments = arguments.ToList().AsReadOnly();
        }

        /// <inheritdoc />
        public override string ToString() => $"Command: {Attribute.Name ?? Type.Name}";
    }
}