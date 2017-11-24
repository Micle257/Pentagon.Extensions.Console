// -----------------------------------------------------------------------
//  <copyright file="COORD.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Native.Structures
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct COORD
    {
        internal short X;
        internal short Y;
    }
}