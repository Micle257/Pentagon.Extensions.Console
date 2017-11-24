// -----------------------------------------------------------------------
//  <copyright file="WindowStyle.cs">
//   Copyright (c) Michal Pokorn�. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Native.Flags
{
    using System;

    [Flags]
    public enum WindowStyle
    {
        Sysmenu = 0x80000,
        SizeBox = 0x40000,
        MaximizeBox = 0x10000,
        MinimizeBox = 0x20000
    }
}