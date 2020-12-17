// -----------------------------------------------------------------------
//  <copyright file="ConsoleColour.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.ColorSystem
{
    using System;
    using JetBrains.Annotations;
    using Pentagon.Extensions.Console;

    public struct ConsoleColour 
    {
        public ConsoleColour(ConsoleColorSlot? foreground, ConsoleColorSlot? background = null)
        {
            Foreground = foreground ?? ConsoleColorSlot.Color15;
            Background = background ?? ConsoleColorSlot.Color00;
        }

        public ConsoleColorSlot Foreground { get; }

        public ConsoleColorSlot Background { get; }

        public override string ToString() => $"Fore: {Foreground}; Back: {Background}";

        [Pure]
        public ConsoleColour WithBackground(ConsoleColorSlot background)
        {
            if (background.Equals(Background))
                return this;
            return new ConsoleColour(Foreground, background);
        }
    }
}