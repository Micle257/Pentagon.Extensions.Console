// -----------------------------------------------------------------------
//  <copyright file="Panel.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Buffers
{
    using System;
    using System.Collections.Generic;
    using Controls;

    public abstract class Panel
    {
        public event EventHandler<IEnumerable<WriteObject>> ControlCleanupRequested;

        public event EventHandler<Control> ControlRedrawRequested;

        public event EventHandler<CursorEventArgs> CursorChanged;

        public IList<Control> Children { get; } = new List<Control>();

        public virtual void AddControl(Control control)
        {
            Children.Add(control);

            control.SizeChanged += (s, e) => OnControlRedrawRequested(control);

            control.RedrawRequested += (s, e) => OnControlRedrawRequested(control);

            control.CleanRequested += (s, e) => OnControlCleanupRequested(e);

            control.PositionChanged += (s, e) => OnControlRedrawRequested(control);

            control.CursorChanged += (s, e) => OnCursorChanged(e);
        }

        protected virtual void OnControlRedrawRequested(Control control)
        {
            ControlRedrawRequested?.Invoke(this, control);
        }

        protected virtual void OnControlCleanupRequested(IEnumerable<WriteObject> objects)
        {
            ControlCleanupRequested?.Invoke(this, objects);
        }

        protected virtual void OnCursorChanged(CursorEventArgs args)
        {
            CursorChanged?.Invoke(this, args);
        }
    }
}