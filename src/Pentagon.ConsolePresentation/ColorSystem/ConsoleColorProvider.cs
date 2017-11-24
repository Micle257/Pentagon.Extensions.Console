namespace Pentagon.Utilities.Console.Structures {
    using System.Collections.Generic;
    using ColorSystem;

    public class ConsoleColorProvider : IConsoleColorProvider
    {
        public IConsoleColorProvider WithBlankColor(ConsoleColour color)
        {
            BlankColor = color;
            return this;
        }

        public IConsoleColorProvider WithTextColor(ConsoleColour color)
        {
            TextColor = color;
            return this;
        }

        public ConsoleColour BlankColor { get; private set; }

        public ConsoleColour TextColor { get; private set; }

        public IList<int> Codes { get; private set; }

        public ConsoleColour GetColour(object foreground, object background)
        {
            return new ConsoleColour(foreground, background);
        }

        public void Initialize<TTheme>()
        {
            Codes = new ColorSchemeManager().GetEnumThemeCodes<TTheme>();
        }
    }
}