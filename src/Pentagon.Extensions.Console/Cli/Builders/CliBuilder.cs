namespace Pentagon.Extensions.Console.Cli.Builders {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    class CliBuilder : ICliBuilder
    {
        [NotNull]
        List<ICliCommandBuilder> _commandBuilders = new List<ICliCommandBuilder>();

        bool _hasImplicitRoot;

        /// <inheritdoc />
        public ICliCommandBuilder<T> HasCommand<T>()
        {
            var cliCommandBuilder = new CliCommandBuilder<T>();

            _commandBuilders.Add(cliCommandBuilder);

            return cliCommandBuilder;
        }

        /// <inheritdoc />
        public ICliCommandBuilder HasCommand(Type type)
        {

            var cliCommandBuilder = new CliCommandBuilder(type);

            _commandBuilders.Add(cliCommandBuilder);

            return cliCommandBuilder;
        }

        /// <inheritdoc />
        public ICliBuilder HasImplicitRoot()
        {
            _hasImplicitRoot = true;

            return this;
        }

        public IReadOnlyCollection<CliCommandDescriber> Build()
        {
            //if (_hasImplicitRoot)
            //{
            //    var cliCommandBuilder = new CliCommandBuilder(null).IsRoot();
            //
            //    _commandBuilders.Add(cliCommandBuilder);
            //
            //    foreach (var builder in _commandBuilders)
            //    {
            //        builder.IsSubCommandFor<>()
            //    }
            //}

            return _commandBuilders.Select(a => a.Build()).ToList().AsReadOnly();
        }
    }
}