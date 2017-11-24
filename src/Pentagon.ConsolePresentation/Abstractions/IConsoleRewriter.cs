﻿// -----------------------------------------------------------------------
//  <copyright file="IConsoleRewriter.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Abstractions
{
    using Buffers;

    public interface IConsoleRewriter
    {
        IConsoleRewriter WithScreen(IScreen screen);

        bool Rewrite(WriteObject writeObject);
    }
}