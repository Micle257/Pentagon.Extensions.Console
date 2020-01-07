// -----------------------------------------------------------------------
//  <copyright file="CliArgumentAttribute.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System;

    public class CliArgumentAttribute : Attribute
    {
        public CliArgumentAttribute() { }

        public CliArgumentAttribute(string name)
        {
            Name = name;
        }

        public int Order { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public bool IsRequired { get; set; } = true;

        public int MinumumNumberOfValuesOverride { get; set; } = -1;

        public int MaximumNumberOfValues { get; set; } = 1;

        public bool IsHidden { get; set; }
    }
}