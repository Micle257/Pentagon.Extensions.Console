// -----------------------------------------------------------------------
//  <copyright file="ConsoleMode.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Native.Flags
{
    using System;

    [Flags]
    public enum ConsoleMode
    {
        EchoInput = 0x4,
        ExtendedFlags = 0x80,
        InsertMode = 0x20,
        LineInput = 0x2,
        MouseInput = 0x10,
        ProcessedInput = 0x1,
        QuickEditMode = 0x40,
        WindowInput = 0x8,
        ProcessedOutput = 0x1,
        WrapAtLineEnd = 0x2,
        VirtualTerminalProcessing = 0x4
    }
}