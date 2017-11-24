// -----------------------------------------------------------------------
//  <copyright file="COLORREF.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Native.Structures
{
    using System.Runtime.InteropServices;
    using Design.Coloring;

    [StructLayout(LayoutKind.Sequential)]
    public struct COLORREF
    {
        internal uint ColorDWORD;

        internal COLORREF(Colour color)
        {
            ColorDWORD = (uint) (color.Rgb.Red + ((uint) color.Rgb.Green << 8) + ((uint) color.Rgb.Blue << 16));
        }

        internal COLORREF(uint r, uint g, uint b)
        {
            ColorDWORD = r + (g << 8) + (b << 16);
        }

        //internal Color GetColor() => Color.FromArgb((int) (0x000000FFU & ColorDWORD),
        //                                            (int) (0x0000FF00U & ColorDWORD) >> 8,
        //                                            (int) (0x00FF0000U & ColorDWORD) >> 16);

        //internal void SetColor(Color color)
        //{
        //    ColorDWORD = color.R + ((uint) color.G << 8) + ((uint) color.B << 16);
        //}
    }
}