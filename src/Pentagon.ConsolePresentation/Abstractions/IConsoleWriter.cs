// -----------------------------------------------------------------------
//  <copyright file="IConsoleWriter.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Abstractions
{
    using Buffers;

    public interface IConsoleWriter
    {
        bool Write(IScreen screen, WriteObject writeObject);
    }
}