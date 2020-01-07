namespace Pentagon.Extensions.Console.Cli {
    using System.Reflection;

    public class CliArgumentDescriber
    {
        public CliArgumentDescriber( PropertyInfo propertyInfo, CliArgumentAttribute attribute)
        {
            PropertyInfo = propertyInfo;
            Attribute    = attribute;
        }

        public PropertyInfo PropertyInfo { get; }

        public CliArgumentAttribute Attribute { get; }
    }
}