// -----------------------------------------------------------------------
//  <copyright file="CliConsoleColor.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console
{
    using System;

    public struct CliConsoleColor : IEquatable<CliConsoleColor>
    {
        public CliConsoleColor(ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            Foreground = foreground;
            Background = background;
        }

        public ConsoleColor? Foreground { get; }

        public ConsoleColor? Background { get; }

        #region Operators

        public static bool operator ==(CliConsoleColor left, CliConsoleColor right) => left.Equals(right);

        public static bool operator !=(CliConsoleColor left, CliConsoleColor right) => !left.Equals(right);

        #endregion

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is CliConsoleColor other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Foreground.GetHashCode() * 397) ^ Background.GetHashCode();
            }
        }

        /// <inheritdoc />
        public bool Equals(CliConsoleColor other) => Foreground == other.Foreground && Background == other.Background;
    }
}