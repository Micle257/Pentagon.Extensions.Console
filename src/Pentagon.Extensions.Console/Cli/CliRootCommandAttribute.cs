// -----------------------------------------------------------------------
//  <copyright file="CliRootCommandAttribute.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CliRootCommandAttribute : CliCommandAttribute
    {
        public CliRootCommandAttribute() { }

        public CliRootCommandAttribute(string description)
        {
            Description = description;
        }
    }
}