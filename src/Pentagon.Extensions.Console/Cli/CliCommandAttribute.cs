// -----------------------------------------------------------------------
//  <copyright file="CliCommandAttribute.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CliCommandAttribute : Attribute
    {
        public CliCommandAttribute() { }

        public CliCommandAttribute(string name)
        {
            Name = name;
        }

        public CliCommandAttribute(string name, string description) : this(name)
        {
            Description = description;
        }

        public string Name { get; set; }

        public string[] AlternedNames { get; set; }

        public string Description { get; set; }
    }
}