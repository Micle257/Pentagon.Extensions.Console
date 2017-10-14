// -----------------------------------------------------------------------
//  <copyright file="ConsoleWriter.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console
{
    using System;
    using Abstractions;
    using Buffers;
    using Helpers;
    using Structures;

    public class ConsoleWriter : IConsoleWriter
    {
        readonly IBufferFilter _bufferFilter;
        readonly IConsoleColorProvider _colorProvider;

        public ConsoleWriter(IBufferFilter bufferFilter, IConsoleColorProvider colorProvider)
        {
            _bufferFilter = bufferFilter;
            _colorProvider = colorProvider;
        }

        public bool Write(IScreen screen, WriteObject writeObject)
        {
            if (screen == null)
                throw new ArgumentNullException(nameof(screen));

            if (!screen.IsActive)
                return false;

            if (writeObject.CanWrite)
            {
                var initalCursorPoint = screen.Cursor.Coord;

                screen.Cursor.Coord = writeObject.StartPoint;
                
                ConsoleHelper.SetColor(writeObject.Color, _colorProvider);

                var chars = _bufferFilter.Filter(screen, writeObject.Characters, writeObject.Elevation);

                foreach (var tuple in chars)
                {
                    if (!screen.CanContain(tuple.Point))
                        continue;

                    screen.Cursor.Coord = tuple.Point;
                    Console.Write(tuple.Character);
                }

                screen.Cursor.Coord = initalCursorPoint;
                writeObject.Write();
                return true;
            }

            return false;
        }
    }
}