// -----------------------------------------------------------------------
//  <copyright file="IScreen.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Buffers
{
    using System.Collections.Generic;
    using Controls;
    using Structures;

    public interface IScreen
    {
        Cursor Cursor { get; }
        IList<WriteObject> Objects { get; }
        IDictionary<BufferPoint, SortedList<int,BufferCell>> CellCache { get; }
        Panel Panel { get; }
        int Width { get; set; }
        int Height { get; set; }
        string Title { get; set; }
        bool IsActive { get; set; }
        IScreen WithPanel(Panel panel);
        bool CanContain(Box box);
        void WriteObject(WriteObject writeObject);
        void CleanObject(WriteObject writeObject);
        void DrawControl(Control control);
        bool CanContain(BufferPoint point);
        void FocusControl(Control control);
        void ApplyColorTheme();
        void Activate();
    }
}