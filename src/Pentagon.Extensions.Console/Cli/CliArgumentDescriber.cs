// -----------------------------------------------------------------------
//  <copyright file="CliArgumentDescriber.cs">
//   Copyright (c) Michal Pokorn�. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System.Reflection;

    public class CliArgumentDescriber
    {
        public CliArgumentDescriber(MemberInfo propertyInfo, CliArgumentAttribute attribute)
        {
            PropertyInfo = propertyInfo;
            Attribute    = attribute;
        }

        public MemberInfo PropertyInfo { get; }

        public CliArgumentAttribute Attribute { get; }
    }
}