namespace Pentagon.Extensions.Console.Cli {
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class SubCliCommandAttribute : Attribute
    {
        public Type Type { get; }

        public SubCliCommandAttribute(Type type)
        {
            Type = type;
        }
    }
}