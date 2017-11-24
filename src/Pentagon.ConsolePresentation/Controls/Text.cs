// -----------------------------------------------------------------------
//  <copyright file="Text.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Buffers;
    using ColorSystem;
    using Structures;

    /// <summary> Represents a printed text on the buffer of the console window. </summary>
    public class Text
    {
        public static List<Text> BufferText = new List<Text>();
        public static Text Current;
        public List<Text> Chars = new List<Text>();

        public Text(object input, ConsoleColour color, BufferPoint coord = default(BufferPoint), bool moveCursor = true)
        {
            Data = input?.ToString() ?? "";
            Color = color;
            Coord = coord;
            MoveCursor = moveCursor;
        }

        public Text(object data, ConsoleColour color, int x, int y) : this(data, color, new BufferPoint(x, y)) { }

        public event EventHandler Printed;
        public static ConsoleWindow Window => ConsoleWindow.CurrentWindow;

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
        public ConsoleColour Color { get; set; }
        public string Data { get; set; }
        public bool MoveCursor { get; set; }

        public static void ClearAll()
        {
            BufferText.Clear();
            Console.Clear();
        }

        public static Text Write(object input, ConsoleColour color, BufferPoint coord, bool moveCursor)
        {
            var obj = new Text(input, color, coord, moveCursor);
            obj.Print();
            return Current;
        }

        public static Text Write(object input, ConsoleColour color, int x, int y) => Write(input, color, new BufferPoint(x, y), true);

        public static Text Write(object input, ConsoleColour color) => Write(input, color, 0, 0);

     //   public static Text Write(object input) => Write(input, ConsoleColours.Text, 0, 0);

       // public static Text Write() => Write(input: "");

        public static Text WriteLine()
        {
            Console.Write(value: "\n");
            return Current;
        }

        public static Text WriteLine(int count)
        {
            for (var i = 0; i < count; i++)
                Console.Write(value: "\n");
            return Current;
        }

        public virtual Text Print()
        {
            // Write(Data, Color, Coord, MoveCursor);
            Print(Data, Color, Coord, MoveCursor);
            return this;
        }

        public override string ToString() => Data;

        public Text Line()
        {
            Console.Write(value: "\n");
            return Current;
        }

        public Text Print(object input, ConsoleColour color, BufferPoint coord, bool moveCursor)
        {
            if (input == null)
                return null;

            var curpos = Window?.Cursor?.Coord ?? new BufferPoint(1, 1);
            Window.Cursor.Coord = coord;
            if (coord.X + input.ToString().Length - 1 > Window.CurrentScreen.Width)
            {
                coord = Window.Cursor.Coord;
             //   color = ConsoleColours.DBlue;
            }

         //    Console.BackgroundColor = Color.Background; //(ConsoleColor) Math.Floor((double) color / 16);
            Console.Write(input?.ToString() ?? "");
            Console.ResetColor();
            if (!moveCursor)
                Window.Cursor.Coord = curpos;

            for (var i = 0; i < Data.Length; i++)
                Chars.Add(new Text(Data[i], Color, new BufferPoint(X + i, Y)));
            Current = this;
            BufferText.Add(Current);

            return Current;
            //Color = color;
            //Print(input);
            //return this;
        }

        public Text Print(object input, ConsoleColour color)
        {
            Color = color;
            Data = input.ToString();
            Print();
            return this;
        }

        public Text Print(object input)
        {
            Data = input.ToString();
            Print();
            return this;
        }

        public Text Print(ConsoleColour color)
        {
            Color = color;
            Print();
            return this;
        }

        public void Clear()
        {
            var off = 0;
            //if ((int)Color > 15) off = 1;
       //     Write(new string(' ', Data.Length + off), ConsoleColours.Blank, X - off, Y);
            if (!MoveCursor)
                Window.Cursor.Coord = Coord;
        }

     //   public void ClearLine() => Write(new string(' ', Window.Width), ConsoleColours.Blank, 1, Y);

        public bool Overlaps(Text text)
        {
            foreach (var item in text.Chars.Select(c => c.Coord))
            {
                if (Chars.Select(c => c.Coord).Any(a => a.X == item.X && a.Y == item.Y))
                    return true;
            }
            return false;
        }
    }
}