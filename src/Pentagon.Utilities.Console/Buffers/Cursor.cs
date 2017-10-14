// -----------------------------------------------------------------------
//  <copyright file="Cursor.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Buffers
{
    using System;
    using Helpers;
    using Pentagon.Extensions;
    using Structures;

    /// <summary> Represent a typing cursor in the console buffer. </summary>
    public class Cursor
    {
        readonly IScreen _screen;
        int _x = 1;

        int _y = 1;

        BufferPoint _coord = new BufferPoint(1, 1);

        bool _show;

        int _size = 25;

        public Cursor(IScreen screen)
        {
            _screen = screen;
        }

        public int X
        {
            get => _x;
            set
            {
                if (value < 0 || value > _screen.Width)
                    _x = value.Mod(_screen.Width) + 1;
                else
                    _x = value;

                _coord = new BufferPoint(_x, _y);

                if (_screen.IsActive)
                    ConsoleHelper.Run(() => Console.CursorLeft = _x - 1);
            }
        }

        public int Y
        {
            get => _y;
            set
            {
                if (value < 0 || value > _screen.Height)
                    _y = value.Mod(_screen.Height);
                else
                    _y = value;

                _coord = new BufferPoint(_x, _y);

                if (_screen.IsActive)
                    ConsoleHelper.Run(() => Console.CursorTop = _y - 1);
            }
        }

        public BufferPoint Coord
        {
            get => _coord;
            set
            {
                X = value.X;
                Y = value.Y;

                _coord = new BufferPoint(_x, _y);
            }
        }

        public bool Show
        {
            get => _show;
            set
            {
                _show = value;

                if (_screen.IsActive)
                    ConsoleHelper.Run(() => Console.CursorVisible = _show);
            }
        }

        public int Size
        {
            get => _size;
            set
            {
                if (value > 0 && value <= 100)
                    _size = value;
                else
                    throw new ArgumentOutOfRangeException(nameof(value), message: "Size of cursor must be in range 0 to 100.");

                if (_screen.IsActive)
                    ConsoleHelper.Run(() => Console.CursorSize = _size);
            }
        }

        public void Refresh()
        {
            if (!_screen.IsActive)
                return;

            Console.CursorLeft = _x - 1;
            Console.CursorTop = _y - 1;
            Console.CursorSize = _size;
            Console.CursorVisible = _show;
        }

        public Cursor Offset(int x, int y, bool canMove)
        {
            if (canMove)
            {
                if (X + x < 1)
                    X = (X + x - 1).Mod(_screen.Width + 1);
                else if (X + x > _screen.Width)
                    X = (X + x).Mod(_screen.Width);
                else
                    X = (X + x).Mod(_screen.Width + 1);

                if (Y + y < 1)
                    Y = (Y + y - 1).Mod(_screen.Height + 1);
                else if (Y + y > _screen.Height)
                    Y = (Y + y).Mod(_screen.Height);
                else
                    Y = (Y + y).Mod(_screen.Height + 1);
            }
            else
            {
                X += x;
                Y += y;
            }
            return this;
        }

        public Cursor Offset(int x, int y) => Offset(x, y, false);

        public Cursor NextLine()
        {
            Y++;
            X = 1;
            return this;
        }
    }
}