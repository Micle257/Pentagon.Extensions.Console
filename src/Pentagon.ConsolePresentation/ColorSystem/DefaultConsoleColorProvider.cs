// -----------------------------------------------------------------------
//  <copyright file="DefaultConsoleColorProvider.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.ColorSystem
{
    using Design.Coloring;

    public static class DefaultConsoleColorProvider
    {
        public static Colour Black { get; } = new Colour(format: "000000");

        public static Colour White { get; } = new Colour(format: "FFFFFF");

        public static Colour Blue { get; } = new Colour(format: "0000ff");

        public static Colour Cyan { get; } = new Colour(format: "00ffff");

        public static Colour DarkBlue { get; } = new Colour(format: "000080");

        public static Colour DarkCyan { get; } = new Colour(format: "008080");

        public static Colour DarkGray { get; } = new Colour(format: "808080");

        public static Colour DarkGreen { get; } = new Colour(format: "008000");

        public static Colour DarkMagenta { get; } = new Colour(format: "800080");

        public static Colour DarkRed { get; } = new Colour(format: "800000");

        public static Colour Yellow { get; } = new Colour(format: "ffff00");

        public static Colour Gray { get; } = new Colour(format: "c0c0c0");

        public static Colour DarkYellow { get; } = new Colour(format: "808000");

        public static Colour Green { get; } = new Colour(format: "00ff00");

        public static Colour Magenta { get; } = new Colour(format: "ff00ff");

        public static Colour Red { get; } = new Colour(format: "ff0000");
    }
}