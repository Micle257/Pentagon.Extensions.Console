// -----------------------------------------------------------------------
//  <copyright file="IColorManager.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.ColorSystem
{
    using System;
    using System.Collections.Generic;
    using Design.Coloring;

    public interface IColorManager
    {
        IDictionary<ConsoleColor, Colour> ColorMapping { get; }
        Colour GetConsoleColor(ConsoleColor consoleColor);
        void SetTheme<TTheme>();
    }
}