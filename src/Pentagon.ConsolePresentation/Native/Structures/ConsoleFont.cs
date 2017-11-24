// -----------------------------------------------------------------------
//  <copyright file="ConsoleFont.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Native.Structures
{
    using System.Runtime.InteropServices;

    /// <summary> Contains information for a console font. 'CONSOLE_FONT_INFO' </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    public struct ConsoleFont
    {
        public uint FontIndex;
        public short FontWidth;
        public short FontHeight;
    }
}