// -----------------------------------------------------------------------
//  <copyright file="StdHandle.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Native.Flags
{
    public enum StdHandle
    {
        /// <summary> The standard input device. Initially, this is the console input buffer, CONIN$. </summary>
        InputHandle = -10,

        /// <summary> The standard output device. Initially, this is the active console screen buffer, CONOUT$. </summary>
        OutputHandle = -11,

        /// <summary> The standard error device. Initially, this is the active console screen buffer, CONOUT$. </summary>
        ErrorHandle = -12
    }
}