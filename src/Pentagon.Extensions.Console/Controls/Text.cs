// -----------------------------------------------------------------------
//  <copyright file="Text.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Controls
{
    using System;
    using ColorSystem;
    using Extensions.Console;
    using Extensions.Console.Structures;

    /// <summary> Represents a printed text on the buffer of the console window. </summary>
    public readonly struct Text
    {
        public Text(object input, ConsoleColour color, BufferPoint? coord = default)
        {
            Data = input?.ToString() ?? "";
            Color = color;
            Coord = coord ?? new BufferPoint(Console.CursorLeft +1 ,Console.CursorTop + 1);
        }

        public Text(object data, ConsoleColour color, int x, int y) : this(data, color, new BufferPoint(x, y)) { }

        public int X => Coord.X;

        public int Y => Coord.Y;

        public BufferPoint Coord { get;  }

        public ConsoleColour Color { get;  }

        public string Data { get;  }

        public override string ToString() => Data;
    }
}