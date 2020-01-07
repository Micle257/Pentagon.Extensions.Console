// -----------------------------------------------------------------------
//  <copyright file="CliCommandDescriber.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CliCommandDescriber : IEquatable<CliCommandDescriber>
    {
        public CliCommandDescriber(Type type, CliCommandAttribute attribute, IEnumerable<CliOptionDescriber> options, IEnumerable<CliArgumentDescriber> arguments)
        {
            Type      = type;
            Attribute = attribute;
            Options   = options.ToList().AsReadOnly();
            Arguments = arguments.ToList().AsReadOnly();
        }

        public Type Type { get; }

        public CliCommandAttribute Attribute { get; }

        public bool IsRoot => Attribute is CliRootCommandAttribute;

        public IReadOnlyCollection<CliOptionDescriber> Options { get; }

        public IReadOnlyCollection<CliArgumentDescriber> Arguments { get; }

        #region Operators

        public static bool operator ==(CliCommandDescriber left, CliCommandDescriber right) => Equals(left, right);

        public static bool operator !=(CliCommandDescriber left, CliCommandDescriber right) => !Equals(left, right);

        #endregion

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((CliCommandDescriber) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => Type != null ? Type.GetHashCode() : 0;

        /// <inheritdoc />
        public bool Equals(CliCommandDescriber other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(Type, other.Type);
        }

        /// <inheritdoc />
        public override string ToString() => $"Command: {Attribute.Name ?? Type.Name}";
    }
}