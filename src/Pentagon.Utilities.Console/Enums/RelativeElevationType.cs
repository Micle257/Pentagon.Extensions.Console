// -----------------------------------------------------------------------
//  <copyright file="RelativeElevationType.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Enums
{
    using System;

    [Flags]
    enum RelativeElevationType
    {
        Unspecified,
        Below,
        Above,
        Equal,
        All = Below | Above | Equal
    }
}