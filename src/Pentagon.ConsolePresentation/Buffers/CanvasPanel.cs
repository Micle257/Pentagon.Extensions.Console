﻿// -----------------------------------------------------------------------
//  <copyright file="CanvasPanel.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Buffers
{
    using Controls;

    public class CanvasPanel : Panel
    {
        public override void AddControl(Control control)
        {
            base.AddControl(control);

            control.IsPositioningEnabled = true;
        }
    }
}