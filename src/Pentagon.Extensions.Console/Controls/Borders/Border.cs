// -----------------------------------------------------------------------
//  <copyright file="Border.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Controls.Borders
{
    using System;
    using Enums;
    using Structures;

    public readonly struct Border : IEquatable<Border>
    {
        public Border(BorderLine allLines)
        {
            TopLine = LeftLine = RightLine = BottomLine = allLines;
        }

        public Border(BorderLine top, BorderLine left, BorderLine right, BorderLine bottom)
        {
            TopLine = top;
            LeftLine = left;
            RightLine = right;
            BottomLine = bottom;
        }

        public Thickness Thickness => new Thickness
                                      {
                                          Left = LeftLine.LineThickness,
                                          Top = TopLine.LineThickness,
                                          Right = RightLine.LineThickness,
                                          Bottom = BottomLine.LineThickness
                                      };

        public BorderLine TopLine { get;  }
        public BorderLine LeftLine { get;  }
        public BorderLine RightLine { get; }
        public BorderLine BottomLine { get; }

        #region Operators

        /// <inheritdoc />
        public static bool operator ==(Border left, Border right) => left.Equals(right);

        /// <inheritdoc />
        public static bool operator !=(Border left, Border right) => !left.Equals(right);

        #endregion

        #region IEquatable members

        /// <inheritdoc />
        public bool Equals(Border other) => TopLine.Equals(other.TopLine) && LeftLine.Equals(other.LeftLine) && RightLine.Equals(other.RightLine) && BottomLine.Equals(other.BottomLine);

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is Border border && Equals(border);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TopLine.GetHashCode();
                hashCode = (hashCode * 397) ^ LeftLine.GetHashCode();
                hashCode = (hashCode * 397) ^ RightLine.GetHashCode();
                hashCode = (hashCode * 397) ^ BottomLine.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}