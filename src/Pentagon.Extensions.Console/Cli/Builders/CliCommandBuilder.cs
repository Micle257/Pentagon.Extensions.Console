namespace Pentagon.Extensions.Console.Cli.Builders {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using JetBrains.Annotations;

    class CliCommandBuilder : ICliCommandBuilder
    {
        [NotNull]
        protected readonly HashSet<string> Names = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        protected string Description;

        [NotNull]
        protected readonly List<(MemberInfo, CliOptionAttribute)> OptionBuilders = new List<(MemberInfo, CliOptionAttribute)>();

        [NotNull]
        protected readonly List<(MemberInfo, CliArgumentAttribute)> ArgumentBuilders = new List<(MemberInfo, CliArgumentAttribute)>();

        bool _isRoot;
        [NotNull]
        readonly List<Type> _subTypes = new List<Type>();

        Type _parentType;

        public CliCommandBuilder(Type type)
        {
            Type = type;
        }

        /// <inheritdoc />
        public Type Type { get; }

        public ICliCommandBuilder HasAttribute(CliCommandAttribute attribute)
        {
            AddName(attribute.Name);

            if (attribute.AlternedNames != null && attribute.AlternedNames.Length > 0)
            {
                foreach (var alternedName in attribute.AlternedNames)
                {
                    Names.Add(alternedName);
                }
            }

            Description = attribute.Description;

            if (attribute.ParentType != null)
                _parentType = attribute.ParentType;

            if (attribute.SubTypes != null && attribute.SubTypes.Length > 0)
                _subTypes.AddRange(attribute.SubTypes);

            return this;
        }

        void AddName(string name)
        {
            // TODO make name from type by convention
            var normalizedName = name ?? Type.Name;

            Names.Add(name);
        }

        public ICliCommandBuilder HasOption(string memberName, CliOptionAttribute attribute)
        {
            // TODO consider multiple with the same name
            MemberInfo prop = Type.GetProperty(memberName);

            if (prop == null)
                prop = Type.GetField(memberName);

            if (prop == null)
                throw new ArgumentException($"Property or field ({memberName}) not found in type {Type}.");

            OptionBuilders.Add((prop, attribute));

            return this;
        }

        /// <inheritdoc />
        public ICliCommandBuilder HasOption(string memberName, Action<CliOptionAttribute> configure = null)
        {
            var attribute = new CliOptionAttribute();

            configure?.Invoke(attribute);

            return HasOption(memberName, attribute);
        }

        public ICliCommandBuilder HasArgument(string memberName, CliArgumentAttribute attribute)
        {
            // TODO consider multiple with the same name
            MemberInfo prop = Type.GetProperty(memberName);

            if (prop == null)
                prop = Type.GetField(memberName);

            if (prop == null)
                throw new ArgumentException($"Property or field ({memberName}) not found in type {Type}.");

            ArgumentBuilders.Add((prop, attribute));

            return this;
        }

        /// <inheritdoc />
        public ICliCommandBuilder HasArgument(string memberName, Action<CliArgumentAttribute> configure = null)
        {
            var attribute = new CliArgumentAttribute();

            configure?.Invoke(attribute);

            return HasArgument(memberName, attribute);
        }

        /// <inheritdoc />
        public ICliCommandBuilder WithName(string name)
        {
            Names.Add(name);

            return this;
        }

        /// <inheritdoc />
        public ICliCommandBuilder WithDescription(string description)
        {
            Description = description;

            return this;
        }

        /// <inheritdoc />
        public ICliCommandBuilder IsRoot()
        {
            _isRoot = true;

            return this;
        }

        /// <inheritdoc />
        public ICliCommandBuilder HasSubCommand(Type subType)
        {
            _subTypes.Add(subType);

            return this;
        }

        /// <inheritdoc />
        public ICliCommandBuilder IsSubCommandFor(Type type)
        {
            _parentType = type;

            return this;
        }

        /// <inheritdoc />
        public CliCommandDescriber Build()
        {
            var attribute = new CliCommandAttribute(Names.FirstOrDefault(), Description)
                            {
                                    AlternedNames = Names.Count > 1 ? Names.Skip(1).ToArray() : null
                            };

            if (_isRoot)
            {
                attribute = new CliRootCommandAttribute(Description)
                            {
                                    Name = Names.FirstOrDefault()
                            };
            }

            if (_subTypes.Count > 0)
            {
                attribute.SubTypes = _subTypes.ToArray();
            }

            if (_parentType != null)
            {
                attribute.ParentType = _parentType;
            }

            var options = OptionBuilders.Select(a => new CliOptionDescriber(a.Item1, a.Item2));

            var arguments = ArgumentBuilders.Select(a => new CliArgumentDescriber(a.Item1, a.Item2));

            return new CliCommandDescriber(Type, attribute, options, arguments);
        }
    }
}