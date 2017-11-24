// -----------------------------------------------------------------------
//  <copyright file="ConsoleFontInfoEx.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Native.Structures
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class ConsoleFontInfoEx
    {
        public readonly int cbSize = Marshal.SizeOf(typeof(ConsoleFontInfoEx));

        public int FontIndex;
        public short FontWidth;
        public short FontHeight;
        public int FontFamily;
        public int FontWeight;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FaceName;
    }
}