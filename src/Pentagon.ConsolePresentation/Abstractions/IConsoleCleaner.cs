// -----------------------------------------------------------------------
//  <copyright file="IConsoleCleaner.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console
{
    using Buffers;

    public interface IConsoleCleaner
    {
        bool Remove(IScreen screen, WriteObject writeObject);
    }
}