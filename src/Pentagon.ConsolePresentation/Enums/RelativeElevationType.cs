// -----------------------------------------------------------------------
//  <copyright file="RelativeElevationType.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Enums
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