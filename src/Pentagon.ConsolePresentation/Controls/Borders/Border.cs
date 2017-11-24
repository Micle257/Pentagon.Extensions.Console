// -----------------------------------------------------------------------
//  <copyright file="Border.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Controls.Borders
{
    using System;
    using Structures;

    public struct Border : IEquatable<Border>
    {
        public Border(BorderLine allLines)
        {
            TopLine = LeftLine = RightLine = BottomLine = allLines;
        }

        public Thickness Thickness => new Thickness
                                      {
                                          Left = LeftLine.Type == BorderLineType.Unspecified ? 0 : 1,
                                          Top = TopLine.Type == BorderLineType.Unspecified ? 0 : 1,
                                          Right = RightLine.Type == BorderLineType.Unspecified ? 0 : 1,
                                          Bottom = BottomLine.Type == BorderLineType.Unspecified ? 0 : 1
                                      };

        public BorderLine TopLine { get; set; }
        public BorderLine LeftLine { get; set; }
        public BorderLine RightLine { get; set; }
        public BorderLine BottomLine { get; set; }

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