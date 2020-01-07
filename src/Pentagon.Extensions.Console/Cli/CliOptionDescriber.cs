// -----------------------------------------------------------------------
//  <copyright file="CliOptionDescriber.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System.Reflection;

    public class CliOptionDescriber
    {
        public CliOptionDescriber(PropertyInfo propertyInfo, CliOptionAttribute attribute)
        {
            PropertyInfo = propertyInfo;
            Attribute    = attribute;
        }

        public PropertyInfo PropertyInfo { get; }

        public CliOptionAttribute Attribute { get; }
    }
}