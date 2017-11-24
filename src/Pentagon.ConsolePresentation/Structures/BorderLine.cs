// -----------------------------------------------------------------------
//  <copyright file="BorderLine.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Structures
{
    using System;
    using ColorSystem;
    using Controls.Borders;

    public struct BorderLine : IValueDataType<BorderLine>
    {
        public BorderLine(BorderLineType type, ConsoleColour? color = null)
        {
            Type = type;
            Color = color ?? ConsoleColour.Text;
            HasValue = true;
        }

        public ConsoleColour Color { get; set; }

        public BorderLineType Type { get; set; }
        public bool HasValue { get; }

        #region Operators

        /// <inheritdoc />
        public static bool operator <(BorderLine left, BorderLine right) => left.CompareTo(right) < 0;

        /// <inheritdoc />
        public static bool operator >(BorderLine left, BorderLine right) => left.CompareTo(right) > 0;

        /// <inheritdoc />
        public static bool operator <=(BorderLine left, BorderLine right) => left.CompareTo(right) <= 0;

        /// <inheritdoc />
        public static bool operator >=(BorderLine left, BorderLine right) => left.CompareTo(right) >= 0;

        /// <inheritdoc />
        public static bool operator ==(BorderLine left, BorderLine right) => left.Equals(right);

        /// <inheritdoc />
        public static bool operator !=(BorderLine left, BorderLine right) => !left.Equals(right);

        #endregion

        #region IEquatable members

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

        #endregion

        /// <inheritdoc />
        public int CompareTo(BorderLine other)
        {
            var typeComparison = Type.CompareTo(other.Type);
            if (typeComparison != 0)
                return typeComparison;
            return Color.CompareTo(other.Color);
        }

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
                return 1;
            if (!(obj is BorderLine))
                throw new ArgumentException($"Object must be of type {nameof(BorderLine)}");
            return CompareTo((BorderLine) obj);
        }
    }
}