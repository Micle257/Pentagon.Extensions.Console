// -----------------------------------------------------------------------
//  <copyright file="CliOptionAttribute.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    public class CliOptionAttribute : Attribute
    {
        public CliOptionAttribute() { }

        public CliOptionAttribute(params string[] aliases)
        {
            Aliases = aliases;
        }

        [CanBeNull]
        public ICollection<string> Aliases { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public bool IsHidden { get; set; }

        public bool IsRequired { get; set; }

        public int ArgumentMaximumNumberOfValues { get; set; } = 1;
    }
}