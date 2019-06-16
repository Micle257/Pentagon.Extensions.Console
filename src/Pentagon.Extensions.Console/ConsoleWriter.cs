// -----------------------------------------------------------------------
//  <copyright file="ConsoleWriter.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console
{
    using System;
    using ConsolePresentation.Controls;
    using JetBrains.Annotations;
    using Structures;

    public static class ConsoleWriter
    {
        public static void Write([NotNull] Text text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var initialCursor = Cursor.Current;

            Cursor.SetCurrent(text.Coord);
            ConsoleHelper.Write(text.Data, text.Color);
            Cursor.SetCurrent(initialCursor);
        }

        public static void Clear([NotNull] Text text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var initialCursor = Cursor.Current;

            Cursor.SetCurrent(text.Coord);
            ConsoleHelper.Write(new string(' ', text.Data.Length), Cli.ColorScheme.Blank);
            Cursor.SetCurrent(initialCursor);
        }

        public static void Write(object data, CliConsoleColor color, int x, int y)
        {
            Write(new Text(data, color, new BufferPoint(x, y)));
        }
    }
}