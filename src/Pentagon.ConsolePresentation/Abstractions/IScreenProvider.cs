// -----------------------------------------------------------------------
//  <copyright file="IScreenProvider.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Abstractions
{
    using Buffers;

    public interface IScreenProvider
    {
        IScreen Create<T>();
    }
}