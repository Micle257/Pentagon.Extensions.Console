// -----------------------------------------------------------------------
//  <copyright file="ConsoleWriter.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable DelegateSubtraction
namespace Pentagon.Extensions.Console
{
    using System;
    using ConsolePresentation.Controls;
    using JetBrains.Annotations;
    using Structures;

    public static class ConsoleWriter
    {
        public static void Line()
        {
            Console.WriteLine();
        }

        public static void WriteSuccess(object successValue) => Write(successValue, CliContext.ColorScheme.Success);

        public static void WriteError(object errorValue) => Write(errorValue, CliContext.ColorScheme.Error);

        public static void WriteWarning(object warningValue) => Write(warningValue, CliContext.ColorScheme.Warning);

        public static void Write(object value, CliConsoleColor color)
        {
            Write(new Text(value, color));
        }

        public static void Write(object value, ConsoleColor? foreColor = null, ConsoleColor? backColor = null)
        {
            Write(value, new CliConsoleColor(foreColor, backColor));
        }

        public static void Write(object data, CliConsoleColor color, int x, int y)
        {
            Write(new Text(data, color, new BufferPoint(x, y)));
        }

        public static void Write([NotNull] Text text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var initialCursor = Cursor.Current;

            var move = text.Coord != initialCursor.Coord;

            if (move)
                Cursor.SetCurrent(text.Coord);

            var fore = Console.ForegroundColor;
            var back = Console.BackgroundColor;

            if (text.Color.Foreground.HasValue)
                Console.ForegroundColor = text.Color.Foreground.Value;
            if (text.Color.Background.HasValue)
                Console.BackgroundColor = text.Color.Background.Value;

            Console.Write(text.Data);

            if (text.Color.Foreground.HasValue)
                Console.ForegroundColor = fore;
            if (text.Color.Background.HasValue)
                Console.BackgroundColor = back;

            if (move)
                Cursor.SetCurrent(initialCursor);

            Wrote?.Invoke(null, text);
        }

        public static EventHandler<Text> Wrote;

        public static void Clear([NotNull] Text text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var initialCursor = Cursor.Current;

            Cursor.SetCurrent(text.Coord);
            Write(new string(' ', text.Data.Length), CliContext.ColorScheme.Blank);
            Cursor.SetCurrent(initialCursor);
        }
    }
}