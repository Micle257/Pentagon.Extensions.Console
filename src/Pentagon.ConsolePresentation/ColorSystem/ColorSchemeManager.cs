namespace Pentagon.Utilities.Console.ColorSystem {
    using System;
    using System.Collections.Generic;

    public class ColorSchemeManager : IColorSchemeManager
    {
        public IList<int> GetEnumThemeCodes<T>()
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(message: "The type must be an enum.");

            var value = Enum.GetValues(typeof(T));

            if (value.Length != 16)
                throw new ArgumentException(message: "The enum must have 16 items.");

            var hexCodes = new List<int>();
            foreach (var o in value)
            {
                    hexCodes.Add((int)o);
            }

            return hexCodes;
        }

        public int GetThemeCode<T>(object value)
        {
           return Convert.ToInt32(value);
        }
    }
}