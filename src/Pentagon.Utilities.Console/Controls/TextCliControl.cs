// -----------------------------------------------------------------------
//  <copyright file="TextCliControl.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Controls
{
    using System;
    using Helpers;

    public class TextCliControl : CliControl<string>
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

        public override string Run()
        {
            Write();
            var read = ConsoleHelper.Read();

            var remoteLength = read.Length;

            for (int i = 0; i < remoteLength + _helperText.Length; i++)
                Console.Write(value: "\b \b");

            if (string.IsNullOrWhiteSpace(read))
                read = _defaultValue;

            ConsoleHelper.Write(read, ConsoleColor.DarkCyan);
            Console.WriteLine();
            return read;
        }

        protected override void Write()
        {
            ConsoleHelper.Write(value: "? ", foreColor: ConsoleColor.DarkGreen);
            ConsoleHelper.Write(_text, ConsoleColor.White);
            if (!string.IsNullOrEmpty(_defaultValue))
                ConsoleHelper.Write(_helperText, ConsoleColor.Gray);
            Console.Write(value: " ");
        }
    }
}