﻿// -----------------------------------------------------------------------
//  <copyright file="ConsoleContext.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Buffers
{
    using System.Threading.Tasks;

    public abstract class ConsoleContext
    {
        public abstract Task Inicialize();
    }
}