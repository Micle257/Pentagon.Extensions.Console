namespace Pentagon.Extensions.Console.Cli {
    using System;
    using System.Collections.Generic;

    public class CliRootCommandDescriber : CliCommandDescriber
    {
        public new CliRootCommandAttribute Attribute { get; }

        public CliRootCommandDescriber( Type type, CliRootCommandAttribute attribute, IEnumerable<CliOptionDescriber> options, IEnumerable<CliArgumentDescriber> arguments) : base(type, attribute,options, arguments)
        {
            Attribute = attribute;
        }
    }
}