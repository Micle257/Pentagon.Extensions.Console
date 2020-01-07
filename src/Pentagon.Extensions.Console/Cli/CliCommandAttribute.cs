namespace Pentagon.Extensions.Console.Cli {
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
   public  class CliCommandAttribute : Attribute
    {
        public string Name { get; set; }

        public string[] AlternedNames { get; set; }

        public string Description { get; set; }

        public CliCommandAttribute()
        {

        }

        public CliCommandAttribute(string name)
        {
            Name = name;
        }

        public CliCommandAttribute(string name, string description) : this(name)
        {
            Description = description;
        }
    }
}