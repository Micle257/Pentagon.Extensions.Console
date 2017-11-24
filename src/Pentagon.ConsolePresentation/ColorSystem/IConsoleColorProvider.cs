namespace Pentagon.ConsolePresentation.ColorSystem {
    using System.Collections.Generic;

    public interface IConsoleColorProvider {
        IConsoleColorProvider WithBlankColor(ConsoleColour color);
        IConsoleColorProvider WithTextColor(ConsoleColour color);
        ConsoleColour BlankColor { get; }
        ConsoleColour TextColor { get; }
        IList<int> Codes { get; }
        ConsoleColour GetColour(object foreground, object background);
        void Initialize<TTheme>();
    }
}