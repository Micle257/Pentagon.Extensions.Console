namespace Pentagon.Utilities.Console {
    using System;
    using Helpers;

    public class TextCliControl
    {
        readonly string _text;
        readonly string _defaultValue;

        public TextCliControl(string text, string defaultValue)
        {
            _text = text;
            _defaultValue = defaultValue;
        }

        void Write()
        {
            ConsoleHelper.Write("? ", ConsoleColor.DarkGreen);
            ConsoleHelper.Write(_text, ConsoleColor.White);
            if (!string.IsNullOrEmpty(_defaultValue))
                ConsoleHelper.Write($" ({_defaultValue}) ", ConsoleColor.Gray);
        }

        public string Run()
        {
            Write();
            var read = ConsoleHelper.Read();

            var remoteLength = read.Length;

            for (int i = 0; i < remoteLength+6; i++)
            {
                Console.Write("\b \b");
            }

            if (string.IsNullOrWhiteSpace(read))
                read = _defaultValue;

            ConsoleHelper.Write(read, ConsoleColor.DarkCyan);
            Console.WriteLine();
            return read;
        }
    }
}