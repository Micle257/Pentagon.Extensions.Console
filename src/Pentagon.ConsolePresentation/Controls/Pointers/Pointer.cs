// -----------------------------------------------------------------------
//  <copyright file="Pointer.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Controls.Pointers
{
    using System;
    using Buffers;
    using Inputs;
    using Structures;

    /// <summary> Represents a cursor pointer that can be moved with. </summary>
    public class Pointer
    {
        readonly bool m_curShow;
        readonly int m_curSize;
        readonly string m_title;

        readonly ConsoleKey UpKey = ConsoleKey.UpArrow;
        readonly ConsoleKey DownKey = ConsoleKey.DownArrow;
        readonly ConsoleKey RightKey = ConsoleKey.RightArrow;
        readonly ConsoleKey LeftKey = ConsoleKey.LeftArrow;

        public Pointer(PointerMoveRule moveRule, BufferPoint startPos, bool canmove)
        {
            StartPos = startPos;
            CurrentPos = StartPos;
            MoveRule = moveRule;
            CanCrossBounds = canmove;
            IsActive = true;
            CursorSize = 100;
            ShowCurrentPos = false;
            m_curShow = Window.CurrentScreen.Cursor.Show;
            m_curSize = Window.CurrentScreen.Cursor.Size;
            m_title = Window.Title;
        }

        public event EventHandler<BufferPoint> Finished;
        public event EventHandler<PointerMoveDirection> Moved;
        public static ConsoleWindow Window => ConsoleWindow.CurrentWindow;

        public BufferPoint StartPos { get; }
        public bool CanCrossBounds { get; }
        public BufferPoint CurrentPos { get; set; }
        public PointerMoveRule MoveRule { get; set; }
        public bool IsActive { get; private set; }
        public int CursorSize { get; set; }
        public bool ShowCurrentPos { get; set; }

        public void Cancel()
        {
            IsActive = false;
            Window.CurrentScreen.Cursor.Show = m_curShow;
            Window.CurrentScreen.Cursor.Size = m_curSize;
            Finished?.Invoke(this, CurrentPos);
        }

        public void Run()
        {
            Window.CurrentScreen.Cursor.Coord = CurrentPos;
            Window.CurrentScreen.Cursor.Show = true;
            Window.CurrentScreen.Cursor.Size = CursorSize;

            while (IsActive)
            {
                if (ShowCurrentPos)
                    Window.Title = $"{m_title} -- Pointer -- X = {CurrentPos.X}   Y = {CurrentPos.Y}";

                var keyHit = Input.WaitKey();

                if (keyHit.Key == ConsoleKey.Escape)
                {
                    Window.CurrentScreen.Cursor.Show = m_curShow;
                    Window.CurrentScreen.Cursor.Size = m_curSize;
                    IsActive = false;
                }
                if (keyHit.Key == ConsoleKey.Enter)
                {
                    IsActive = false;
                    Window.CurrentScreen.Cursor.Show = m_curShow;
                    Window.CurrentScreen.Cursor.Size = m_curSize;
                    Finished?.Invoke(this, CurrentPos);
                }
                try
                {
                    if (keyHit.Key == UpKey && (MoveRule & PointerMoveRule.Up) == PointerMoveRule.Up)
                    {
                        Window.CurrentScreen.Cursor.Offset(0, -1, CanCrossBounds);
                        Moved?.Invoke(this, PointerMoveDirection.Up);
                    }
                    else if (keyHit.Key == DownKey && (MoveRule & PointerMoveRule.Down) == PointerMoveRule.Down)
                    {
                        Window.CurrentScreen.Cursor.Offset(0, 1, CanCrossBounds);
                        Moved?.Invoke(this, PointerMoveDirection.Down);
                    }
                    else if (keyHit.Key == RightKey && (MoveRule & PointerMoveRule.Right) == PointerMoveRule.Right)
                    {
                        Window.CurrentScreen.Cursor.Offset(1, 0, CanCrossBounds);
                        Moved?.Invoke(this, PointerMoveDirection.Right);
                    }
                    else if (keyHit.Key == LeftKey && (MoveRule & PointerMoveRule.Left) == PointerMoveRule.Left)
                    {
                        Window.CurrentScreen.Cursor.Offset(-1, 0, CanCrossBounds);
                        Moved?.Invoke(this, PointerMoveDirection.Left);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
                CurrentPos = Window.CurrentScreen.Cursor.Coord;
            }
        }
    }
}