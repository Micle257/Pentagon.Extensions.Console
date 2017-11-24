// -----------------------------------------------------------------------
//  <copyright file="ConsoleRewriter.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console
{
    using System;
    using Buffers;

    public class ConsoleRewriter : IConsoleRewriter
    {
        public IConsoleRewriter WithScreen(IScreen screen) => throw new NotImplementedException();

        public bool Rewrite(WriteObject writeObject) => throw new NotImplementedException();
    }
}