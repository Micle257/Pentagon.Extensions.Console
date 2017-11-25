namespace Pentagon.Utilities.Console {
    using System;
    using Helpers;

    public class SwitchCliControl
    {
        readonly string _text;
        readonly bool _defaultValue;

        public SwitchCliControl(string text, bool defaultValue)
        {
            _text = text;
            _defaultValue = defaultValue;
        }

        void Write()
        {
            ConsoleHelper.Write("? ", ConsoleColor.DarkGreen);
            ConsoleHelper.Write(_text, ConsoleColor.White);
            if (_defaultValue)
                ConsoleHelper.Write(" (Y/n) ", ConsoleColor.Gray);
            else
                ConsoleHelper.Write(" (y/N) ", ConsoleColor.Gray);
        }

        bool? ProccessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return _defaultValue;

            if (input.Equals("y", StringComparison.OrdinalIgnoreCase))
                return true;
            else if (input.Equals("n", StringComparison.OrdinalIgnoreCase))
                return false;

            return null;
        }

        public bool Run()
        {
            Write();
            bool? result = null;
            while (result == null)
            {
                var read = ConsoleHelper.Read();
                result = ProccessInput(read);
                var remoteLength = read.Length;
                for (int i = 0; i < remoteLength; i++)
                {
                    Console.Write("\b \b");
                }
            }

            for (int i = 0; i < 6; i++)
            {
                Console.Write("\b \b");
            }

            ConsoleHelper.Write(result.Value ? "Yes" : "No", ConsoleColor.DarkCyan);
            Console.WriteLine();
            return result.Value;
        }
    }
}