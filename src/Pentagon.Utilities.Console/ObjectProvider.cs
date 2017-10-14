// -----------------------------------------------------------------------
//  <copyright file="ObjectProvider.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Buffers;
    using Enums;
    using Pentagon.Helpers;
    using Structures;

    class ObjectProvider
    {
        readonly IScreen _screen;
        WriteStatus _status;
        RelativeElevationType _elevationType;
        int _elevation;
        Func<char, bool> _characterPredicate;

        public ObjectProvider(IScreen screen)
        {
            _screen = screen;
        }

        public IEnumerable<(int elevation, IEnumerable<(char character, BufferPoint point, ConsoleColour color)>)> GetGroupedObjects()
        {
            var filteredObjects = GetFilteredObjects();

            return filteredObjects.GroupBy(a => a.Elevation)
                                  .Select(a => (a.Key, a.SelectMany(b => b.Characters.Select(c => (character: c.character, point: c.point, color: b.Color))).GroupBy(b => b.point).Select(b => b.First())))
                                  .OrderByDescending(a => a.Item1);
        }

        public IEnumerable<(char character, BufferPoint point, ConsoleColour color)> GetTopMostObjects()
        {
            var group = GetGroupedObjects();

            var result = new List<(char, BufferPoint, ConsoleColour)>();

            foreach (var pnt in group)
            {
                foreach (var p in pnt.Item2)
                {
                    if (!result.Select(a => a.Item2).Any(a => a == p.point))
                        yield return p;
                }
            }
        }

        IEnumerable<WriteObject> GetFilteredObjects()
        {
            Func<int, bool> equalPredicate = i => i == _elevation;
            Func<int, bool> abovePredicate = i => i < _elevation;
            Func<int, bool> belowPredicate = i => i > _elevation;

            Func<int, bool> predicate = i =>
                                        {
                                            return _elevationType.HasValue(RelativeElevationType.Equal) && equalPredicate(i)
                                                   || _elevationType.HasValue(RelativeElevationType.Below) && belowPredicate(i)
                                                   || _elevationType.HasValue(RelativeElevationType.Above) && abovePredicate(i);
                                        };

            return _screen.Objects.Where(o => o.Status == _status).Where(o => predicate(o.Elevation));
        }
    }
}