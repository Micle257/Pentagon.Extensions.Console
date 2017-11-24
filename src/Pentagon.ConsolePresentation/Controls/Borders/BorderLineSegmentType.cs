// -----------------------------------------------------------------------
//  <copyright file="BorderLineSegmentType.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Controls.Borders
{
    using System;

    [Flags]
    public enum BorderLineSegmentType
    {
        Unspecified,
        Horizontal,
        Vertical,
        TopLeftCorner,
        TopRightCorner,
        BottomLeftCorner,
        BottomRightCorner,

        //TODO ...
        Any = Horizontal | Vertical | TopLeftCorner | TopRightCorner | BottomLeftCorner | BottomRightCorner
    }
}