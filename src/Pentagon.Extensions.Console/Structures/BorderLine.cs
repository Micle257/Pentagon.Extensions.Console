// -----------------------------------------------------------------------
//  <copyright file="BorderLine.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Structures
{
    using System;
    using Enums;

    public readonly struct BorderLine : IEquatable<BorderLine>
    {
        public BorderLine(BorderLineType type, CliConsoleColor? color = null)
        {
            Type = type;
            LineThickness = type == BorderLineType.None ? 0 : 1;
            Color = color ?? new CliConsoleColor();
        }

        public BorderLine(BorderLineType type, int lineThickness, CliConsoleColor? color = null)
        {
            if (lineThickness < 0)
                throw new ArgumentOutOfRangeException(nameof(lineThickness), lineThickness, "Border line thickness must be grater than or equal to zero.");

            Type          = type;
            LineThickness = lineThickness;
            Color         = color ?? new CliConsoleColor();
        }

        public CliConsoleColor Color { get; }

        public BorderLineType Type { get; }

        public int LineThickness { get; }

        #region Operators

        /// <inheritdoc />
        public static bool operator ==(BorderLine left, BorderLine right) => left.Equals(right);

        /// <inheritdoc />
        public static bool operator !=(BorderLine left, BorderLine right) => !left.Equals(right);

        #endregion

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

        /// <inheritdoc />
        public bool Equals(BorderLine other) => Color == other.Color && Type == other.Type;
    }
}