// -----------------------------------------------------------------------
//  <copyright file="TextMetricPitchAndFamily.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Native.Flags
{
    public enum TextMetricPitchAndFamily : uint
    {
        TMPF_FIXED_PITCH = 0x1,
        TMPF_VECTOR = 0x2,
        TMPF_TRUETYPE = 0x4,
        TMPF_DEVICE = 0x8
    }
}