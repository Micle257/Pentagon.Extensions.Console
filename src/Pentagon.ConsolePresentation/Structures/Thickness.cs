// -----------------------------------------------------------------------
//  <copyright file="Thickness.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Structures
{
    using System;

    public struct Size // TODO bitch
    {
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; set; }

        public int Height { get; set; }
    }

    public struct Thickness : IValueDataType<Thickness>
    {
        public Thickness(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
            HasValue = true;
        }

        public Thickness(int horizontal, int vertical) : this(horizontal, vertical, horizontal, vertical) { }

        public Thickness(int all) : this(all, all) { }

        public static Thickness Zero => new Thickness();

        public int Vertical => Top + Bottom;

        public int Horizontal => Left + Right;

        public int Left { get; set; }

        public int Top { get; set; }

        public int Right { get; set; }

        public int Bottom { get; set; }

        public bool HasValue { get; }

        #region Operators

        /// <inheritdoc />
        public static bool operator <(Thickness left, Thickness right) => left.CompareTo(right) < 0;

        /// <inheritdoc />
        public static bool operator >(Thickness left, Thickness right) => left.CompareTo(right) > 0;

        /// <inheritdoc />
        public static bool operator <=(Thickness left, Thickness right) => left.CompareTo(right) <= 0;

        /// <inheritdoc />
        public static bool operator >=(Thickness left, Thickness right) => left.CompareTo(right) >= 0;

        /// <inheritdoc />
        public static bool operator ==(Thickness left, Thickness right) => left.Equals(right);

        /// <inheritdoc />
        public static bool operator !=(Thickness left, Thickness right) => !left.Equals(right);

        #endregion

        #region IEquatable members

        /// <inheritdoc />
        public bool Equals(Thickness other) => Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is Thickness t && Equals(t);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Left;
                hashCode = (hashCode * 397) ^ Top;
                hashCode = (hashCode * 397) ^ Right;
                hashCode = (hashCode * 397) ^ Bottom;
                return hashCode;
            }
        }

        #endregion

        /// <inheritdoc />
        public int CompareTo(Thickness other)
        {
            var leftComparison = Left.CompareTo(other.Left);
            if (leftComparison != 0)
                return leftComparison;
            var topComparison = Top.CompareTo(other.Top);
            if (topComparison != 0)
                return topComparison;
            var rightComparison = Right.CompareTo(other.Right);
            if (rightComparison != 0)
                return rightComparison;
            return Bottom.CompareTo(other.Bottom);
        }

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
                return 1;
            if (!(obj is Thickness))
                throw new ArgumentException($"Object must be of type {nameof(Thickness)}");
            return CompareTo((Thickness) obj);
        }

        /// <inheritdoc />
        public override string ToString() => $"Left: {Left}, Top: {Top}, Right: {Right}, Bottom: {Bottom}";
    }
}