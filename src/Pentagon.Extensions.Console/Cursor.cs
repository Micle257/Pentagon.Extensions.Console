// -----------------------------------------------------------------------
//  <copyright file="Cursor.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console
{
    using System;
    using System.IO;
    using JetBrains.Annotations;
    using Structures;
    using SC = System.Console;

    /// <summary> Represent a typing cursor in the console buffer. </summary>
    public class Cursor
    {
        public Cursor(int x, int y, bool show, int size)
        {
            X = x;
            Y = y;
            Coord = new BufferPoint(x, y);

            Show = show;

            if (size > 0 && size <= 100)
                Size = size;
            else
                throw new ArgumentOutOfRangeException(nameof(size), message: @"Size of cursor must be in range 0 to 100.");
        }

        [NotNull]
        public static Cursor Current => new Cursor(SC.CursorLeft + 1, SC.CursorTop + 1, SC.CursorVisible, SC.CursorSize);

        public int X { get; }

        public int Y { get; }

        public BufferPoint Coord { get; }

        public bool Show { get; }

        public int Size { get; }

        public static void SetCurrent(int x, int y, bool show, int size)
        {
            var cursor = new Cursor(x, y, show, size);

            cursor.Apply();
        }

        public static void SetCurrent([NotNull] Action<CursorMeta> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var cursor = Current.Copy(configure);

            cursor.Apply();
        }

        public static void SetCurrent(BufferPoint coord)
        {
            Current.Copy(a => a.Coord = coord).Apply();
        }

        public static void SetCurrent(Cursor cursor)
        {
            cursor.Apply();
        }

        public void Apply()
        {
            SC.CursorLeft = X - 1;
            SC.CursorTop = Y - 1;

            if (OS.Platform == OperatingSystemPlatform.Windows)
            {
                SC.CursorSize = Size;
                SC.CursorVisible = Show;
            }
        }

        [NotNull]
        public Cursor Offset(int x, int y, bool canMove)
        {
            return Copy(c =>
                        {
                            var xx = c.Coord.X;
                            var yy = c.Coord.Y;

                            if (canMove)
                            {
                                if (xx + x < 1)
                                    xx = (xx + x - 1).Mod(SC.WindowWidth + 1);
                                else if (xx + x > SC.WindowWidth)
                                    xx = (xx + x).Mod(SC.WindowWidth);
                                else
                                    xx = (xx + x).Mod(SC.WindowWidth + 1);

                                if (yy + y < 1)
                                    yy = (yy + y - 1).Mod(SC.WindowHeight + 1);
                                else if (yy + y > SC.WindowHeight)
                                    yy = (yy + y).Mod(SC.WindowHeight);
                                else
                                    yy = (yy + y).Mod(SC.WindowHeight + 1);
                            }
                            else
                            {
                                xx += x;
                                yy += y;
                            }

                            c.Coord = new BufferPoint(xx,yy);
                        });
        }

        [NotNull]
        public Cursor Offset(int x, int y) => Offset(x, y, false);

        [NotNull]
        public Cursor EnsureNewLine() => X > 1 ? NextLine() : this;

        [NotNull]
        public Cursor NextLine()
        {
            return Copy(a => a.Coord = new BufferPoint(1, Y + 1));
        }

        public Cursor Copy(Action<CursorMeta> callback)
        {
            var meta = new CursorMeta
            {
                Coord = Coord,
                Show = Show,
                Size = Size
            };

            callback?.Invoke(meta);

            return new Cursor(meta.Coord.X, meta.Coord.Y, meta.Show, meta.Size);
        }

        public class CursorMeta
        {
            public BufferPoint Coord { get; set; }

            public bool Show { get; set; }

            public int Size { get; set; }

            public int X
            {
                get => Coord.X;
                set => Coord = new BufferPoint(value, Coord.Y);
            }

            public int Y
            {
                get => Coord.Y;
                set => Coord = new BufferPoint(Coord.X, value);
            }
        }
    }
}