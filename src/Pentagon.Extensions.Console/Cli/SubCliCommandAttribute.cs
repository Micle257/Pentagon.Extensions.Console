// -----------------------------------------------------------------------
//  <copyright file="SubCliCommandAttribute.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class SubCliCommandAttribute : Attribute
    {
        public SubCliCommandAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}