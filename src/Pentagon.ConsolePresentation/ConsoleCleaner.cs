// -----------------------------------------------------------------------
//  <copyright file="ConsoleCleaner.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console
{
    using System;
    using System.Linq;
    using Abstractions;
    using Buffers;
    using Helpers;
    using Structures;

    public class ConsoleCleaner : IConsoleCleaner
    {
        readonly IBufferFilter _bufferFilter;
        readonly IConsoleColorProvider _colorProvider;

        public ConsoleCleaner(IBufferFilter bufferFilter, IConsoleColorProvider colorProvider)
        {
            _bufferFilter = bufferFilter;
            _colorProvider = colorProvider;
        }

        public bool Remove(IScreen screen, WriteObject writeObject)
        {
            if (!screen.IsActive)
                return false;

            if (writeObject.CanRemove)
            {
                var chars = _bufferFilter.Filter(screen, writeObject.Characters, writeObject.Elevation).ToList();

                foreach (var tuple in chars)
                {
                    screen.Cursor.Coord = tuple.Point;
                    ConsoleHelper.SetColor(tuple.Color, _colorProvider);
                    ConsoleHelper.Run(() => Console.Write(tuple.Character));
                }

                screen.Cursor.X += writeObject.Box.Width;
                writeObject.Remove();
                return true;
            }

            return false;
        }
    }
}