// -----------------------------------------------------------------------
//  <copyright file="SecretTextCliControl.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Controls
{
    using System;
    using System.Security;

    public class SecretTextCliControl : CliControl<SecureString>
    {
        readonly string _text;
        readonly SecretTextOutputMode _outputMode;

        public SecretTextCliControl(string text, SecretTextOutputMode outputMode = SecretTextOutputMode.NoOutput)
        {
            _text = text;
            _outputMode = outputMode;
        }

        public override SecureString Run()
        {
            Write();
            var read = ConsoleHelper.ReadSecret(_outputMode);

            var remoteLength = read.Length;

            if (_outputMode == SecretTextOutputMode.Asterisk || _outputMode == SecretTextOutputMode.PeekLast)
            {
                for (var i = 0; i < remoteLength; i++)
                    Console.Write(value: "\b \b");

                ConsoleHelper.Write(new string('*', read.Length), ConsoleColor.DarkCyan);
            }

            Console.WriteLine();
            return read;
        }

        protected override void Write()
        {
            ConsoleHelper.Write(value: "? ", ConsoleColor.DarkGreen);
            ConsoleHelper.Write(_text, ConsoleColor.White);
            Console.Write(value: " ");
        }
    }
}