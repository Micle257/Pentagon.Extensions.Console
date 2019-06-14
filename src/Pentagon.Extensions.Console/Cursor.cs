// -----------------------------------------------------------------------
//  <copyright file="Cursor.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console
{
    using System;
    using JetBrains.Annotations;
    using Pentagon.Extensions;
    using Structures;
    using SC = System.Console;

    public interface IConsole
    {
        ConsoleSize Size { get; }
    }

    public class SystemConsole : IConsole
    {
        public ConsoleSize Size
        {
            get => new ConsoleSize(SC.WindowWidth, SC.WindowHeight);
            set
            {
                SC.WindowWidth = value.Width;
                SC.WindowHeight = value.Height;
            }
        }
    }

    /// <summary> Represent a typing cursor in the console buffer. </summary>
    public class Cursor
    {
        [NotNull]
        readonly IConsole _console;

        int _x = 1;

        int _y = 1;

        BufferPoint _coord = new BufferPoint(1, 1);

        public Cursor([NotNull] IConsole console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        bool _show;

        int _size = 25;

        public int X
        {
            get => _x;
            set
            {
                _x = NormalizeXCoord(value);
                
                _coord = new BufferPoint(_x, _y);
            }
        }

        void NormalizeCoord()
        {
            X = _x;
            Y = _y;
        }

        int NormalizeXCoord(int value)
        {
            if (value < 0 || value > _console.Size.Width)
                return value.Mod(_console.Size.Width) + 1;

            return value;
        }

        int NormalizeYCoord(int value)
        {
            if (value < 0 || value > _console.Size.Height)
                return value.Mod(_console.Size.Height) + 1;

            return value;
        }

        public int Y
        {
            get => _y;
            set
            {
                _y = NormalizeYCoord(value);
                _coord = new BufferPoint(_x, _y);
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
                    throw new ArgumentOutOfRangeException(nameof(value), message: @"Size of cursor must be in range 0 to 100.");
            }
        }

        public void Apply()
        {
            NormalizeCoord();

            Console.CursorLeft = _x - 1;
            Console.CursorTop = _y - 1;

            if (OS.Platform == OperatingSystemPlatform.Windows)
            {
                Console.CursorSize = _size;
                Console.CursorVisible = _show;
            }

        }

        public Cursor Offset(int x, int y, bool canMove)
        {
            if (canMove)
            {
                if (X + x < 1)
                    X = (X + x - 1).Mod(_console.Size.Width + 1);
                else if (X + x > _console.Size.Width)
                    X = (X + x).Mod(_console.Size.Width);
                else
                    X = (X + x).Mod(_console.Size.Width + 1);

                if (Y + y < 1)
                    Y = (Y + y - 1).Mod(_console.Size.Height + 1);
                else if (Y + y > _console.Size.Height)
                    Y = (Y + y).Mod(_console.Size.Height);
                else
                    Y = (Y + y).Mod(_console.Size.Height + 1);
            }
            else
            {
                X += x;
                Y += y;
            }
            return this;
        }

        public Cursor Offset(int x, int y) => Offset(x, y, false);

        public Cursor EnsureNewLine() => X > 1 ? NextLine() : this;

        public Cursor NextLine()
        {
            Y++;
            X = 1;
            return this;
        }

        public static Cursor FromCurrentPosition(IConsole console)
        {
            var cursor = new Cursor(console);

            cursor.X = SC.CursorLeft + 1;
            cursor.Y = SC.CursorTop + 1;
            cursor.Show = SC.CursorVisible;
            cursor.Size = SC.CursorSize;

            return cursor;
        }
    }
}