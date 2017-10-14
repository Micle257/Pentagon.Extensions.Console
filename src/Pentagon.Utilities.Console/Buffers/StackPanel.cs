// -----------------------------------------------------------------------
//  <copyright file="StackPanel.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Buffers
{
    using System.Collections.Generic;
    using Controls;
    using Enums;
    using Structures;

    public class StackPanel : Panel
    {
        public HorizontalAlignment HorizontalAlignment { get; set; }

        public VerticalAlignment VerticalAlignment { get; set; }

        public Orientation Orientation { get; set; }

        public override void AddControl(Control control)
        {
            base.AddControl(control);
            GetStartPoints();
        }

        protected override void OnControlRedrawRequested(Control control)
        {
            base.OnControlRedrawRequested(control);
            GetStartPoints();
        }

        protected override void OnControlCleanupRequested(IEnumerable<WriteObject> objects)
        {
            base.OnControlCleanupRequested(objects);
            GetStartPoints();
        }

        void GetStartPoints()
        {
            var height = 0;
            var point = BufferPoint.Origin;
            foreach (var control in Children)
            {
                control.SetPoint(point.WithOffset(0, height));
                height += control.Height;
            }
        }
    }
}