namespace Pentagon.Utilities.Console
{
    using System;
    using Helpers;

    public class TextCliControl
    {
        readonly string _text;
        readonly string _defaultValue;
        readonly string _helperText;

        public TextCliControl(string text, string defaultValue = null)
        {
            _text = text;
            _defaultValue = defaultValue ?? string.Empty;
            if (defaultValue != null)
                _helperText = $" ({_defaultValue})";
            else
                _helperText = "";
        }

        void Write()
        {
            ConsoleHelper.Write("? ", ConsoleColor.DarkGreen);
            ConsoleHelper.Write(_text, ConsoleColor.White);
            if (!string.IsNullOrEmpty(_defaultValue))
                ConsoleHelper.Write(_helperText, ConsoleColor.Gray);
            Console.Write(" ");
        }

        public string Run()
        {
            Write();
            var read = ConsoleHelper.Read();

            var remoteLength = read.Length;

            for (int i = 0; i < remoteLength + _helperText.Length; i++)
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