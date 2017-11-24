namespace Pentagon.ConsolePresentation.Controls {
    using System;
    using Structures;

    public class CursorEventArgs : EventArgs
    {
        public CursorEventArgs(int size, bool show)
        {
            Size = size;
            Show = show;
        }

        public CursorEventArgs(BufferPoint point)
        {
            Point = point;
        }

        public int? Size { get; }
        public bool? Show { get; }
        public BufferPoint? Point { get; }
    }
}