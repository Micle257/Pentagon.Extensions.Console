// -----------------------------------------------------------------------
//  <copyright file="Box.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Structures
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public struct Box : IEquatable<Box>
    {
        public Box(BufferPoint point, int width, int height)
        {
            Point = point;
            Width = width;
            Height = height;
            ExcludedBoxes = new Collection<Box>();
        }

        public BufferPoint Point { get; }

        public int Width { get; }

        public int Height { get; }

        public bool IsValid => Point.IsValid && Width > 0 && Height > 0;

        public IEnumerable<BufferPoint> Points => GetFillPoints();

        ICollection<Box> ExcludedBoxes { get; }

        #region Operators

        /// <inheritdoc />
        public static bool operator ==(Box left, Box right) => left.Equals(right);

        /// <inheritdoc />
        public static bool operator !=(Box left, Box right) => !left.Equals(right);

        #endregion

        #region IEquatable members

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Point.GetHashCode();
                hashCode = (hashCode * 397) ^ Width;
                hashCode = (hashCode * 397) ^ Height;
                hashCode = (hashCode * 397) ^ (ExcludedBoxes != null ? ExcludedBoxes.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is Box box && Equals(box);
        }

        /// <inheritdoc />
        public bool Equals(Box other) => Point.Equals(other.Point) && Width == other.Width && Height == other.Height && Equals(ExcludedBoxes, other.ExcludedBoxes);

        #endregion

        public void ExcludeBox(Box boxToExlude)
        {
            if (Points.Intersect(boxToExlude.Points).Count() == boxToExlude.Points.Count())
                ExcludedBoxes.Add(boxToExlude);
        }

        public Box Expand(int widthIncrement, int heighIncrement) => new Box(Point, Width + widthIncrement, Height + heighIncrement);

        public IEnumerable<Box> GetRowBoxes()
        {
            var points = GetPoints().ToList();
            var startPoint = Point;
            for (var i = 0; i < Height; i++)
            {
                var initalBox = new Box(Point.WithOffset(0, i), 0, 1);
                var box = initalBox;
                var point = default(BufferPoint);
                foreach (var p in points.Where(p => p.Y == startPoint.WithOffset(0, i).Y))
                {
                    if (point.IsValid && p.X > point.X + 1)
                    {
                        if (!box.IsValid)
                            throw new ArgumentException(message: "Yielded box is invalid.");

                        yield return box;
                        box = new Box(p, 0, 1);
                    }

                    box = box.Expand(1, 0);
                    point = p;
                }

                if (box.IsValid)
                    yield return box;
            }
        }

        IEnumerable<BufferPoint> GetFillPoints()
        {
            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                    yield return Point.WithOffset(i, j);
            }
        }

        IEnumerable<BufferPoint> GetPoints()
        {
            var excludePoints = ExcludedBoxes.SelectMany(a => a.Points).Distinct();
            return Points.Except(excludePoints);
        }
    }
}