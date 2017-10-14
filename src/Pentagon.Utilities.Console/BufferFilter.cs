// -----------------------------------------------------------------------
//  <copyright file="BufferFilter.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Attributes;
    using Buffers;
    using Structures;

    [Register(RegisterType.Singleton, typeof(IBufferFilter))]
    public class BufferFilter : IBufferFilter
    {
        readonly IConsoleColorProvider _colorProvider;

        public BufferFilter(IConsoleColorProvider colorProvider)
        {
            _colorProvider = colorProvider;
        }

        /// <inheritdoc />
        public IEnumerable<BufferCell> Filter(IScreen screen, IEnumerable<(char, BufferPoint)> characters, int elevation)
        {
            var cache = screen.CellCache;

            var filter = new List<BufferCell>();
            foreach (var c in characters)
            {
                var bufferCell = new BufferCell(c.Item1, c.Item2, default(ConsoleColour), elevation);

                if (cache.TryGetValue(c.Item2, out var value))
                {
                    if (value.Max().Key <= elevation)
                    {
                        filter.Add(bufferCell);
                    }
                }
                else
                {
                    filter.Add(bufferCell);
                }
            }
            return filter;
        }
    }
}