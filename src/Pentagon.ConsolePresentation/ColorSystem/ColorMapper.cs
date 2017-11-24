// -----------------------------------------------------------------------
//  <copyright file="ColorMapper.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.ColorSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Design.Coloring;
    using Native;
    using Native.Flags;
    using Native.Structures;
    using Registration;

    [Register(RegisterType.Transient, typeof(IColorMapper))]
    public class ColorMapper : IColorMapper
    {
        public bool MapColor(ConsoleColor colorPostition, Colour color)
        {
            try
            {
                MapColorCore(colorPostition, color);
                return true;
            }
            catch (Win32Exception e)
            {
                return false;
            }
        }

        public IDictionary<ConsoleColor, COLORREF> GetBufferColors()
        {
            var result = new Dictionary<ConsoleColor, COLORREF>();
            var outputHandle = KernelNativeMethods.GetStdHandle(StdHandle.OutputHandle);
            var csbe = KernelNativeMethods.GetConsoleScreenBufferInfoEx(outputHandle);

            result.Add(ConsoleColor.Black, csbe.black);
            result.Add(ConsoleColor.Blue, csbe.blue);
            result.Add(ConsoleColor.Cyan, csbe.cyan);
            result.Add(ConsoleColor.DarkBlue, csbe.darkBlue);
            result.Add(ConsoleColor.DarkCyan, csbe.darkCyan);
            result.Add(ConsoleColor.DarkGray, csbe.darkGray);
            result.Add(ConsoleColor.DarkGreen, csbe.darkGreen);
            result.Add(ConsoleColor.DarkMagenta, csbe.darkMagenta);
            result.Add(ConsoleColor.DarkRed, csbe.darkRed);
            result.Add(ConsoleColor.White, csbe.white);
            result.Add(ConsoleColor.Yellow, csbe.yellow);
            result.Add(ConsoleColor.Gray, csbe.gray);
            result.Add(ConsoleColor.DarkYellow, csbe.darkYellow);
            result.Add(ConsoleColor.Green, csbe.green);
            result.Add(ConsoleColor.Magenta, csbe.magenta);
            result.Add(ConsoleColor.Red, csbe.red);

            return result;
        }

        public void SetBufferColors(IDictionary<ConsoleColor, COLORREF> colors)
        {
            var outputHandle = KernelNativeMethods.GetStdHandle(StdHandle.OutputHandle);
            var csbe = KernelNativeMethods.GetConsoleScreenBufferInfoEx(outputHandle);

            csbe.black = colors[ConsoleColor.Black];
            csbe.blue = colors[ConsoleColor.Blue];
            csbe.cyan = colors[ConsoleColor.Cyan];
            csbe.darkBlue = colors[ConsoleColor.DarkBlue];
            csbe.darkCyan = colors[ConsoleColor.DarkCyan];
            csbe.darkGray = colors[ConsoleColor.DarkGray];
            csbe.darkGreen = colors[ConsoleColor.DarkGreen];
            csbe.darkMagenta = colors[ConsoleColor.DarkMagenta];
            csbe.darkRed = colors[ConsoleColor.DarkRed];
            csbe.darkYellow = colors[ConsoleColor.DarkYellow];
            csbe.blue = colors[ConsoleColor.Blue];
            csbe.gray = colors[ConsoleColor.Gray];
            csbe.darkBlue = colors[ConsoleColor.DarkBlue];
            csbe.green = colors[ConsoleColor.Green];
            csbe.magenta = colors[ConsoleColor.Magenta];
            csbe.red = colors[ConsoleColor.Red];

            KernelNativeMethods.SetConsoleScreenBufferInfo(outputHandle, ref csbe);
        }

        void MapColorCore(ConsoleColor colorPosition, Colour color)
        {
            var outputHandle = KernelNativeMethods.GetStdHandle(StdHandle.OutputHandle);
            var csbe = KernelNativeMethods.GetConsoleScreenBufferInfoEx(outputHandle);

            switch (colorPosition)
            {
                case ConsoleColor.Black:
                    csbe.black = new COLORREF(color);
                    break;

                case ConsoleColor.Blue:
                    csbe.black = new COLORREF(color);
                    break;

                case ConsoleColor.Cyan:
                    csbe.black = new COLORREF(color);
                    break;

                case ConsoleColor.DarkBlue:
                    csbe.black = new COLORREF(color);
                    break;

                case ConsoleColor.DarkCyan:
                    csbe.black = new COLORREF(color);
                    break;

                case ConsoleColor.DarkGray:
                    csbe.black = new COLORREF(color);
                    break;

                case ConsoleColor.DarkGreen:
                    csbe.black = new COLORREF(color);
                    break;

                case ConsoleColor.DarkMagenta:
                    csbe.black = new COLORREF(color);
                    break;

                case ConsoleColor.DarkRed:
                    csbe.black = new COLORREF(color);
                    break;

                case ConsoleColor.DarkYellow:
                    csbe.black = new COLORREF(color);
                    break;

                case ConsoleColor.Gray:
                    csbe.black = new COLORREF(color);
                    break;

                case ConsoleColor.Green:
                    csbe.black = new COLORREF(color);
                    break;

                case ConsoleColor.Magenta:
                    csbe.black = new COLORREF(color);
                    break;

                case ConsoleColor.Red:
                    csbe.black = new COLORREF(color);
                    break;

                case ConsoleColor.White:
                    csbe.black = new COLORREF(color);
                    break;

                case ConsoleColor.Yellow:
                    csbe.black = new COLORREF(color);
                    break;
            }

            KernelNativeMethods.SetConsoleScreenBufferInfo(outputHandle, ref csbe);
        }
    }
}