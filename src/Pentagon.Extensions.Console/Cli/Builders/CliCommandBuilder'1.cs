namespace Pentagon.Extensions.Console.Cli.Builders {
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    class CliCommandBuilder<TCommand> : CliCommandBuilder, ICliCommandBuilder<TCommand>
    {
        public CliCommandBuilder() : base(typeof(TCommand))
        {

        }

        /// <inheritdoc />
        public new ICliCommandBuilder<TCommand> WithName(string name)
        {
            base.WithName(name);

            return this;
        }

        /// <inheritdoc />
        public new ICliCommandBuilder<TCommand> WithDescription(string description)
        {
            base.WithDescription(description);

            return this;
        }

        /// <inheritdoc />
        public new ICliCommandBuilder<TCommand> IsRoot()
        {
            base.IsRoot();

            return this;
        }

        /// <inheritdoc />
        public ICliCommandBuilder<TCommand> HasSubCommand<T>()
        {
            base.HasSubCommand(typeof(T));

            return this;
        }

        /// <inheritdoc />
        public ICliCommandBuilder<TCommand> IsSubCommandFor<TSub>()
        {
            base.IsSubCommandFor(typeof(TSub));

            return this;
        }

        /// <inheritdoc />
        public ICliCommandBuilder<TCommand> HasOption<T>(Expression<Func<TCommand, T>> property, Action<CliOptionAttribute> configure)
        {
            var attribute = new CliOptionAttribute();

            configure?.Invoke(attribute);

            var prop = GetPropertyTypeFromExpression(property);

            OptionBuilders.Add((prop, attribute));

            return this;
        }

        /// <inheritdoc />
        public ICliCommandBuilder<TCommand> HasArgument<T>(Expression<Func<TCommand, T>> property, Action<CliArgumentAttribute> configure)
        {
            var attribute = new CliArgumentAttribute();

            configure?.Invoke(attribute);

            var prop = GetPropertyTypeFromExpression(property);

            ArgumentBuilders.Add((prop, attribute));

            return this;
        }

        MemberInfo GetPropertyTypeFromExpression<T>(Expression<Func<TCommand, T>> property)
        {
            var propertyBody = property.Body.Reduce();

            if (!(propertyBody is MemberExpression member))
                throw new ArgumentException("Property selector must be selection of property or field.");

            if (!(member.Expression is ParameterExpression) && !(member.Expression is MemberExpression))
                throw new ArgumentException("Property selector must be single access, like: (c => c.Property).");

            return member.Member;
        }
    }
}