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
        readonly string[] _aliases;

        public CliOptionAttribute() { }

        public CliOptionAttribute(params string[] aliases)
        {
            _aliases = aliases;
        }

        [NotNull]
        public IReadOnlyCollection<string> Aliases => _aliases ?? Array.Empty<string>();

        public string Description { get; set; }
        public string Name { get; set; }
        public bool IsHidden { get; set; }
        public bool IsRequired { get; set; }
    }
}