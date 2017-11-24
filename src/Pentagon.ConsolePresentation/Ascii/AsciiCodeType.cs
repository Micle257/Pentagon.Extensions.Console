// -----------------------------------------------------------------------
//  <copyright file="AsciiCodeType.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Ascii
{
    using System.ComponentModel;

    public enum AsciiCodeType
    {
        Unspecified,

        [Description(description: "control")]
        Control,

        [Description(description: "basic")]
        Basic,

        [Description(description: "extended")]
        Extended
    }
}