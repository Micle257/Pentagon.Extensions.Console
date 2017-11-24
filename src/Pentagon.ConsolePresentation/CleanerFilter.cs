// -----------------------------------------------------------------------
//  <copyright file="CleanerFilter.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Buffers;
    using Structures;

    public class CleanerFilter : IBufferFilter
    {
        readonly IConsoleColorProvider _colorProvider;

        public CleanerFilter(IConsoleColorProvider colorProvider)
        {
            _colorProvider = colorProvider;
        }

        public IEnumerable<BufferCell> Filter(IScreen screen, IEnumerable<(char, BufferPoint)> characters, int elevation)
        {
            var cache = screen.CellCache;

            var filter = new List<BufferCell>();
            foreach (var c in characters)
            {
                if (cache.TryGetValue(c.Item2, out var value))
                {
                    if (value.Count > 1)
                    {
                        filter.Add(value[value.Count - 2]);
                    }
                    else filter.Add(new BufferCell(' ', c.Item2, ConsoleColour.Blank, elevation));
                }
                else
                    filter.Add(new BufferCell(' ', c.Item2, ConsoleColour.Blank, elevation));
            }
            return filter;
        }

        /// <inheritdoc />
        IEnumerable<(char Character, BufferPoint Point, ConsoleColour Color)> FilterCharacters(IScreen screen, IEnumerable<(char, BufferPoint)> characters, int elevation)
        {
            // get all written object below elevation
            var objects = screen.Objects.Where(o => o.Status == WriteStatus.Write).Where(o => o.Elevation < elevation);

            // group all object by elevation and valued by iteration of char and point
            var objectPoints = objects.GroupBy(a => a.Elevation)
                                      .Select(a => (a.Key, a.SelectMany(b => b.Characters.Select(c => (character: c.character, point: c.point, color: b.Color))).GroupBy(b => b.point).Select(b => b.First())))
                                      .OrderByDescending(a => a.Item1);

            // filter only the top most points
            var sad = new List<(char, BufferPoint, ConsoleColour)>();

            foreach (var pnt in objectPoints)
            {
                foreach (var p in pnt.Item2)
                {
                    if (!sad.Select(a => a.Item2).Any(a => a == p.point))
                        sad.Add(p);
                }
            }

            // get the objects that are located in characters'
            var writtablePoints = sad.Select(a => a.Item2).Intersect(characters.Select(a => a.Item2));
            var writtable = sad.Where(a => writtablePoints.Any(b => b == a.Item2));

            var diffPoints = characters.Select(a => a.Item2).Except(writtable.Select(a => a.Item2));

            var diff = characters.Where(a => diffPoints.Any(b => b == a.Item2))
                                 .Select(a => (' ', a.Item2))
                                 .Select(a => (a.Item1, a.Item2, _colorProvider.BlankColor));

            return writtable.Concat(diff);
        }
    }
}