namespace Pentagon.Utilities.Console.Ascii {
    using System.Linq;
    using Pentagon.Data.Helpers;
    using Properties;

    public static class Ascii
    {
        static AsciiTable _table;

        public static void Initialize()
        {
            if (_table == null)
                _table = ResourceReader.ReadJson<AsciiTable>(Resources.ASCIITable.Replace(oldValue: "\r\n", newValue: "").Replace(oldValue: "\\", newValue: "").Replace(oldValue: " ", newValue: ""));
        }

        static AsciiCode GetCode(int code) => _table.Codes.FirstOrDefault(c => c.Code == code);

        public static class Control { }

        public static class Basic
        {
            public static AsciiCode I { get; } = GetCode(73);
        }

        public static class Extended
        {
            public static AsciiCode DottedBoxLowDensity { get; } = GetCode(176);
            public static AsciiCode DottedBoxMediumDensity { get; } = GetCode(177);
            public static AsciiCode DottedBoxHighDensity { get; } = GetCode(178);

            public static AsciiCode BoxSingleLineTopRightCorner { get; } = GetCode(191);
            public static AsciiCode BoxSingleLineBottomRightCorner { get; } = GetCode(217);
            public static AsciiCode BoxSingleLineBottomLeftCorner { get; } = GetCode(192);
            public static AsciiCode BoxSingleLineTopLeftCorner { get; } = GetCode(218);
            public static AsciiCode BoxSingleVerticalLine { get; } = GetCode(179);
            public static AsciiCode BoxSingleHorizontalLine { get; } = GetCode(196);

            public static AsciiCode BoxDoubleVerticalLine { get; } = GetCode(186);
            public static AsciiCode BoxDoubleLineTopRightCorner { get; } = GetCode(187);
            public static AsciiCode BoxDoubleLineBottomRightCorner { get; } = GetCode(188);
            public static AsciiCode BoxDoubleLineBottomLeftCorner { get; } = GetCode(200);
            public static AsciiCode BoxDoubleLineTopLeftCorner { get; } = GetCode(201);
            public static AsciiCode BoxDoubleHorizontalLine { get; } = GetCode(205);

            public static AsciiCode Block { get; } = GetCode(219);
            public static AsciiCode BottomHalfBlock { get; } = GetCode(220);
            public static AsciiCode TopHalfBlock { get; } = GetCode(223);
        }
    }
}