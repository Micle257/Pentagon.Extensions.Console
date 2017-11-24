// -----------------------------------------------------------------------
//  <copyright file="PointerMoveRule.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Controls.Pointers
{
    using System;

    /// <summary> Enum representing a user movement posibilities in <see cref="Pointer" />. </summary>
    [Flags]
    public enum PointerMoveRule
    {
        Up = 1,
        Down = 2,
        Right = 4,
        Left = 8,
        Vertical = Up | Down,
        Horizontal = Right | Left,
        All = Vertical | Horizontal
    }
}