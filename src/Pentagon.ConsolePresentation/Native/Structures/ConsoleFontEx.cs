// -----------------------------------------------------------------------
//  <copyright file="ConsoleFontEx.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Native.Structures
{
    using System.Runtime.InteropServices;

    /// <summary> Contains extended information for a console font. 'CONSOLE_FONT_INFOEX' </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    public class ConsoleFontEx
    {
        public uint FontIndex;
        public short FontWidth;
        public short FontHeight;
        public uint FontFamily;
        public uint FontWeight;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FaceName;

        int cbSize;

        public ConsoleFontEx()
        {
            cbSize = Marshal.SizeOf(typeof(ConsoleFontEx));
        }

        public ConsoleFontEx(string fontName, short height) : this()
        {
            FaceName = fontName;
            FontHeight = height;
        }
    }
}