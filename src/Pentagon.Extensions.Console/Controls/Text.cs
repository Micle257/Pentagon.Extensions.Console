// -----------------------------------------------------------------------
//  <copyright file="Text.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Controls
{
    using System;
    using Extensions.Console;
    using Extensions.Console.Structures;

    /// <summary> Represents a printed text on the buffer of the console window. </summary>
    public class Text
    {
        public Text(object input, CliConsoleColor color, BufferPoint? coord = default)
        {
            Data = input?.ToString() ?? "";
            Color = color;
            Coord = coord ?? new BufferPoint(Console.CursorLeft +1 ,Console.CursorTop + 1);
        }

        public Text(object data, CliConsoleColor color, int x, int y) : this(data, color, new BufferPoint(x, y)) { }

        public event EventHandler Printed;

        public int X
        {
            get => Coord.X;
            set => Coord = new BufferPoint(value, Y);
        }

        public int Y
        {
            get => Coord.Y;
            set => Coord = new BufferPoint(X, value);
        }

        public BufferPoint Coord { get; set; }

        public CliConsoleColor Color { get; set; }

        public string Data { get; set; }

        // public static Text Write(object input, CliConsoleColor color, BufferPoint coord, bool moveCursor)
        // {
        //     var obj = new Text(input, color, coord, moveCursor);
        //     obj.Print();
        //     return Current;
        // }
        //
        // public static Text Write(object input, CliConsoleColor color, int x, int y) => Write(input, color, new BufferPoint(x, y), true);
        //
        // public static Text Write(object input, CliConsoleColor color) => Write(input, color, 0, 0);
        //
        // public static Text WriteLine()
        // {
        //     Console.WriteLine();
        //     return Current;
        // }
        //
        // public static Text WriteLine(int count)
        // {
        //     for (var i = 0; i < count; i++)
        //         Console.WriteLine();
        //     return Current;
        // }

        public override string ToString() => Data;

        // Text Print(object input, CliConsoleColor color, BufferPoint coord, bool moveCursor)
        // {
        //     if (input == null)
        //         return null;
        //
        //     var curpos = Cursor.Current;
        //
        //     Cursor.SetCurrent(coord);
        //
        //     if (coord.X + input.ToString().Length - 1 > Console.WindowWidth)
        //     {
        //         coord = Cursor.Current;
        //     }
        //
        //     Console.Write(input?.ToString() ?? "");
        //     Console.ResetColor();
        //
        //     if (!moveCursor)
        //         Window.Cursor.Coord = curpos;
        //
        //     for (var i = 0; i < Data.Length; i++)
        //         Chars.Add(new Text(Data[i], Color, new BufferPoint(X + i, Y)));
        //
        //     BufferText.Add(this);
        //
        //     return this;
        //     //Color = color;
        //     //Print(input);
        //     //return this;
        // }
    }
}