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
        public CliOptionDescriber(MemberInfo propertyInfo, CliOptionAttribute attribute)
        {
            PropertyInfo = propertyInfo;
            Attribute    = attribute;
        }

        public MemberInfo PropertyInfo { get; }

        public CliOptionAttribute Attribute { get; }
    }
}