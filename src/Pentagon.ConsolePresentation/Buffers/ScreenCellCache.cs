// -----------------------------------------------------------------------
//  <copyright file="ScreenCellCache.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Buffers
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Registration;
    using Structures;
    using Enumerable = System.Linq.Enumerable;

    [Register(RegisterType.Transient, typeof(IScreenCellCache))]
    public class ScreenCellCache : IScreenCellCache
    {
        public IDictionary<BufferPoint, SortedList<int,BufferCell>> Cache { get; } = new ConcurrentDictionary<BufferPoint, SortedList<int, BufferCell>>();

        public void AddOrReplaceCell(BufferCell cell)
        {
            if (!Cache.ContainsKey(cell.Point))
            {
                var set = new SortedList<int, BufferCell>();
                set.Add(cell.Elevation,cell);
                Cache.Add(cell.Point, set);
            }
            else
            {
                if (!Cache[cell.Point].ContainsKey(cell.Elevation))
                    Cache[cell.Point].Add(cell.Elevation, cell);
                else
                    Cache[cell.Point][cell.Elevation] = cell;
            }
        }

        public void RemoveCell(BufferPoint point, int elevation)
        {
            if (Cache.ContainsKey(point))
            {
                if (Cache[point].ContainsKey(elevation))
                {
                    Cache[point].Remove(elevation);
                    if (Cache[point].Count == 0)
                        Cache.Remove(point);
                }
            }
        }
    }
}