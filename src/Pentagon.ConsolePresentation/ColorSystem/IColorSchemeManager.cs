namespace Pentagon.ConsolePresentation.ColorSystem {
    using System.Collections.Generic;

    public interface IColorSchemeManager {
        IList<int> GetEnumThemeCodes<T>();
        int GetThemeCode<T>(object value);
    }
}