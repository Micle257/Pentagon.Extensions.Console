namespace Pentagon.Extensions.Console.Cli {
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CliRootCommandAttribute : CliCommandAttribute
    {
        public CliRootCommandAttribute() : base()
        {

        }

        public CliRootCommandAttribute(string description)
        {
            Description = description;
        }
    }
}