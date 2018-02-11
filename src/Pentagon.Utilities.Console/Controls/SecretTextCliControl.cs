// -----------------------------------------------------------------------
//  <copyright file="SecretTextCliControl.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Controls
{
    using System;
    using System.Security;
    using Helpers;

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
            var read = ConsoleHelper.ReadSecret(_outputMode == SecretTextOutputMode.Asterisk);

            var remoteLength = read.Length;

            if (_outputMode == SecretTextOutputMode.Asterisk)
            {
                for (int i = 0; i < remoteLength; i++)
                    Console.Write(value: "\b \b");

                ConsoleHelper.Write(new string('*', read.Length), ConsoleColor.DarkCyan);
            }

            Console.WriteLine();
            return read;
        }

        protected override void Write()
        {
            ConsoleHelper.Write(value: "? ", foreColor: ConsoleColor.DarkGreen);
            ConsoleHelper.Write(_text, ConsoleColor.White);
            Console.Write(value: " ");
        }
    }
}