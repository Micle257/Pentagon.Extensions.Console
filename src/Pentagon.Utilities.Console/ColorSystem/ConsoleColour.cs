// -----------------------------------------------------------------------
//  <copyright file="ConsoleColour.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Structures
{
    using System;
    using Design.Coloring;
    using Design.Coloring.ColorModels;
    using JetBrains.Annotations;

    public struct ConsoleColour : IValueDataType<ConsoleColour>
    {
        readonly byte _colorNumber;
        readonly int _backCode;
        readonly int _foreCode;

        public ConsoleColour(object foreground, object background = null)
        {
            Foreground = foreground;
            Background = background;
            _foreCode = (int) (foreground ?? -1);
            _backCode = (int) (background ?? -1);
            IsText = IsBlank = IsInvert = false;

            _colorNumber = (byte) (_foreCode + (_backCode << 4));
            HasValue = true;
        }

        public static ConsoleColour Blank { get; } = new ConsoleColour {IsBlank = true};
        public static ConsoleColour Text { get; } = new ConsoleColour {IsText = true};

        public static ConsoleColour InvertText { get; } = new ConsoleColour {IsInvert = true};
        public bool IsBlank { get; private set; }

        public bool IsText { get; private set; }

        public bool IsInvert { get; private set; }

        public double ContrastRatio => GetContrastRatio();

        public object Foreground { get; }

        public object Background { get; }

        /// <inheritdoc />
        public bool HasValue { get; }

        #region Operators

        /// <inheritdoc />
        public static bool operator <(ConsoleColour left, ConsoleColour right) => left.CompareTo(right) < 0;

        /// <inheritdoc />
        public static bool operator >(ConsoleColour left, ConsoleColour right) => left.CompareTo(right) > 0;

        /// <inheritdoc />
        public static bool operator <=(ConsoleColour left, ConsoleColour right) => left.CompareTo(right) <= 0;

        /// <inheritdoc />
        public static bool operator >=(ConsoleColour left, ConsoleColour right) => left.CompareTo(right) >= 0;

        /// <inheritdoc />
        public static bool operator ==(ConsoleColour left, ConsoleColour right) => left.Equals(right);

        /// <inheritdoc />
        public static bool operator !=(ConsoleColour left, ConsoleColour right) => !left.Equals(right);

        #endregion

        #region IEquatable members

        /// <inheritdoc />
        public bool Equals(ConsoleColour other) => _colorNumber == other._colorNumber;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is ConsoleColour color && Equals(color);
        }

        /// <inheritdoc />
        public override int GetHashCode() => _colorNumber;

        #endregion

        /// <inheritdoc />
        public int CompareTo(ConsoleColour other) => _colorNumber.CompareTo(other._colorNumber);

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
                return 1;
            if (!(obj is ConsoleColour color))
                throw new ArgumentException($"Object must be of type {nameof(ConsoleColour)}");
            return CompareTo(color);
        }

        public override string ToString() => $"Fore: {Foreground}; Back: {Background}";

        public ColorContrast GetContrast()
        {
            var foreColor = new Colour(new RgbColorModel(_foreCode));
            var backColor = new Colour(new RgbColorModel(_backCode));

            return foreColor.GetContrast(backColor);
        }

        [Pure]
        public ConsoleColour WithBackground(object background)
        {
            if (background.Equals(Background))
                return this;
            return new ConsoleColour(Foreground, background);
        }

        double GetContrastRatio()
        {
            var foreColor = new Colour(new RgbColorModel(_foreCode));
            var backColor = new Colour(new RgbColorModel(_backCode));

            return foreColor.ContrastRatio(backColor);
        }
    }
}