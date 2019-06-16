// -----------------------------------------------------------------------
//  <copyright file="Cursor.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console
{
    using System;
    using JetBrains.Annotations;
    using Structures;
    using SC = System.Console;

    /// <summary> Represent a typing cursor in the console buffer. </summary>
    public class Cursor
    {
        int _x = 1;

        int _y = 1;

        BufferPoint _coord = new BufferPoint(1, 1);

        int _size = 25;

        [NotNull]
        public static Cursor Current => new Cursor
                                        {
                                                X = SC.CursorLeft + 1,
                                                Y = SC.CursorTop + 1,
                                                Show = SC.CursorVisible,
                                                Size = SC.CursorSize
                                        };

        public int X
        {
            get => _x;
            set
            {
                _x = NormalizeXCoord(value);

                _coord = new BufferPoint(_x, _y);
            }
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

        public bool Show { get; set; }

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

        public static void SetCurrent(int x, int y, bool show, int size)
        {
            var cursor = new Cursor
                         {
                                 X = x,
                                 Y = y,
                                 Size = size,
                                 Show = show
                         };

            cursor.Apply();
        }

        public static void SetCurrent([NotNull] Action<Cursor> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var cursor = Current;

            configure(cursor);

            cursor.Apply();
        }

        public static void SetCurrent(BufferPoint coord)
        {
            SetCurrent(c => c.Coord = coord);
        }

        public static void SetCurrent(Cursor cursor)
        {
            cursor.Apply();
        }

        public void Apply()
        {
            NormalizeCoord();

            SC.CursorLeft = _x - 1;
            SC.CursorTop = _y - 1;

            if (OS.Platform == OperatingSystemPlatform.Windows)
            {
                SC.CursorSize = _size;
                SC.CursorVisible = Show;
            }
        }

        public Cursor Offset(int x, int y, bool canMove)
        {
            if (canMove)
            {
                if (X + x < 1)
                    X = (X + x - 1).Mod(SC.WindowWidth + 1);
                else if (X + x > SC.WindowWidth)
                    X = (X + x).Mod(SC.WindowWidth);
                else
                    X = (X + x).Mod(SC.WindowWidth + 1);

                if (Y + y < 1)
                    Y = (Y + y - 1).Mod(SC.WindowHeight + 1);
                else if (Y + y > SC.WindowHeight)
                    Y = (Y + y).Mod(SC.WindowHeight);
                else
                    Y = (Y + y).Mod(SC.WindowHeight + 1);
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

        void NormalizeCoord()
        {
            X = _x;
            Y = _y;
        }

        int NormalizeXCoord(int value)
        {
            if (value < 0 || value > SC.WindowWidth)
                return value.Mod(SC.WindowWidth) + 1;

            return value;
        }

        int NormalizeYCoord(int value)
        {
            if (value < 0 || value > SC.WindowHeight)
                return value.Mod(SC.WindowHeight) + 1;

            return value;
        }
    }
}