// -----------------------------------------------------------------------
//  <copyright file="BorderLine.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Structures
{
    using System;
    using Enums;

    public struct BorderLine : IEquatable<BorderLine>
    {
        public BorderLine(BorderLineType type, CliConsoleColor? color = null)
        {
            Type = type;
            Color = color ?? new CliConsoleColor();
            HasValue = true;
        }

        public CliConsoleColor Color { get; set; }

        public BorderLineType Type { get; set; }

        public bool HasValue { get; }

        /// <inheritdoc />
        public static bool operator ==(BorderLine left, BorderLine right) => left.Equals(right);

        /// <inheritdoc />
        public static bool operator !=(BorderLine left, BorderLine right) => !left.Equals(right);

        /// <inheritdoc />
        public bool Equals(BorderLine other) => Color == other.Color && Type == other.Type;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is BorderLine line && Equals(line);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Color.GetHashCode() * 397) ^ (int) Type;
            }
        }
    }
}