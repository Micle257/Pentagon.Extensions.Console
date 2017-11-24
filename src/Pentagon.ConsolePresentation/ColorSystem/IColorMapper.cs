// -----------------------------------------------------------------------
//  <copyright file="IColorMapper.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.ColorSystem
{
    using System;
    using System.Collections.Generic;
    using Design.Coloring;
    using Native.Structures;

    public interface IColorMapper
    {
        bool MapColor(ConsoleColor colorPostition, Colour color);
        IDictionary<ConsoleColor, COLORREF> GetBufferColors();
        void SetBufferColors(IDictionary<ConsoleColor, COLORREF> colors);
    }
}