// -----------------------------------------------------------------------
//  <copyright file="BufferPoint.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Structures
{
    using System;

    /// <summary> Represent a coord in console buffer. </summary>
    public struct BufferPoint : IValueDataType<BufferPoint>
    {
        /// <summary> Initializes a new instance of the <see cref="BufferPoint" /> class. </summary>
        /// <param name="xcoord"> The X Coord. </param>
        /// <param name="ycoord"> The Y Coord. </param>
        public BufferPoint(int xcoord, int ycoord)
        {
            X = xcoord;
            Y = ycoord;
            HasValue = true;
        }

        public static BufferPoint Zero => default(BufferPoint);
        public static BufferPoint Origin => new BufferPoint(1, 1);

        public static BufferPoint Content { get; set; }

        /// <summary> The horizontal coord of <see cref="BufferPoint" /> </summary>
        public int X { get; }

        /// <summary> The vertical coord of <see cref="BufferPoint" /> </summary>
        public int Y { get; }

        public bool IsValid => X > 0 && Y > 0;

        public bool HasValue { get; }

        #region Operators

        /// <inheritdoc />
        public static bool operator <(BufferPoint left, BufferPoint right) => left.CompareTo(right) < 0;

        /// <inheritdoc />
        public static bool operator >(BufferPoint left, BufferPoint right) => left.CompareTo(right) > 0;

        /// <inheritdoc />
        public static bool operator <=(BufferPoint left, BufferPoint right) => left.CompareTo(right) <= 0;

        /// <inheritdoc />
        public static bool operator >=(BufferPoint left, BufferPoint right) => left.CompareTo(right) >= 0;

        /// <inheritdoc />
        public static bool operator ==(BufferPoint left, BufferPoint right) => left.Equals(right);

        /// <inheritdoc />
        public static bool operator !=(BufferPoint left, BufferPoint right) => !left.Equals(right);

        #endregion

        #region IEquatable members

        /// <inheritdoc />
        public bool Equals(BufferPoint other) => X == other.X && Y == other.Y;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is BufferPoint && Equals((BufferPoint) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        #endregion

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
                return 1;
            if (!(obj is BufferPoint))
                throw new ArgumentException($"Object must be of type {nameof(BufferPoint)}");
            return CompareTo((BufferPoint) obj);
        }

        /// <inheritdoc />
        public int CompareTo(BufferPoint other)
        {
            var xComparison = X.CompareTo(other.X);
            if (xComparison != 0)
                return xComparison;
            return Y.CompareTo(other.Y);
        }

        /// <inheritdoc />
        public override string ToString() => $"{nameof(X)}: {X}, {nameof(Y)}: {Y}";

        /// <summary> Creates a new point with offset from this point. </summary>
        /// <param name="xOffset"> The x offset. </param>
        /// <param name="yOffset"> The y offset. </param>
        /// <returns> A  new offsetted <see cref="BufferPoint" />. </returns>
        public BufferPoint WithOffset(int xOffset, int yOffset) => new BufferPoint(X + xOffset, Y + yOffset);
    }
}