// -----------------------------------------------------------------------
//  <copyright file="IBufferFilter.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Abstractions
{
    using System.Collections.Generic;
    using Buffers;
    using Structures;

    public interface IBufferFilter
    {
        IEnumerable<BufferCell> Filter(IScreen screen, IEnumerable<(char, BufferPoint)> characters, int elevation);
    }
}