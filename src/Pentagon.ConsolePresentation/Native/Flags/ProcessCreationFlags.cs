// -----------------------------------------------------------------------
//  <copyright file="ProcessCreationFlags.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Native.Flags
{
    using System;

    [Flags]
    public enum ProcessCreationFlags : uint
    {
        CreateBreakawayFromJob = 0x01000000,
        CreateDefaultErrorMode = 0x04000000,
        CreateNewConsole = 0x00000010,
        CreateNewProcessGroup = 0x00000200,
        CreateNoWindow = 0x08000000,
        CreateProtectedProcess = 0x00040000,
        CreatePreserveCodeAuthzLevel = 0x02000000,
        CreateSeparateWowVdm = 0x00001000,
        CreateSuspendend = 0x00000004,
        CreateUnicodeEnvironment = 0x00000400,
        DebugOnlyThisProcess = 0x00000002,
        DebugProcess = 0x00000001,
        DetachedProcess = 0x00000008,
        ExtendedStartupinfoPresent = 0x00080000,
        InheritParentAffinity = 0x00010000
    }
}