namespace Pentagon.Utilities.Console {
    using System;
    using System.Security;
    using Helpers;

    public class SecretTextCliControl
    {
        readonly string _text;
        readonly bool _writeAsterisk;

        public SecretTextCliControl(string text, bool writeAsterisk = false)
        {
            _text = text;
            _writeAsterisk = writeAsterisk;
        }

        void Write()
        {
            ConsoleHelper.Write("? ", ConsoleColor.DarkGreen);
            ConsoleHelper.Write(_text, ConsoleColor.White);
            Console.Write(" ");
        }

        public SecureString Run()
        {
            Write();
            var read = ConsoleHelper.ReadSecret(_writeAsterisk);

            var remoteLength = read.Length;

            if (_writeAsterisk)
            {
                for (int i = 0; i < remoteLength; i++)
                {
                    Console.Write("\b \b");
                }

                ConsoleHelper.Write(new string('*', read.Length), ConsoleColor.DarkCyan);
            }
            Console.WriteLine();
            return read;
        }
    }
}