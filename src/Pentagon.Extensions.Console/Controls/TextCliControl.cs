// -----------------------------------------------------------------------
//  <copyright file="TextCliControl.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Controls
{
    using System;

    public class TextCliControl : CliControl<string>
    {
        string _defaultValue;
        string _helperText;

        public string Text { get; set; }

        public string DefaultValue
        {
            get => _defaultValue;
            set
            {
                _defaultValue = value ?? string.Empty;
                _helperText = value != null ? $" ({_defaultValue})" : "";
            }
        }

        public string TypedText { get; set; }

        public override string Run()
        {
            Write();

            var read = ConsoleHelper.Read(TypedText);

            var remoteLength = read.Length;

            for (var i = 0; i < remoteLength + _helperText.Length; i++)
                Console.Write(value: "\b \b");

            if (string.IsNullOrWhiteSpace(read))
                read = _defaultValue;

            ConsoleHelper.Write(read, ConsoleColor.DarkCyan);
            Console.WriteLine();
            return read;
        }

        protected override void Write()
        {
            WriteLabel(Text);

            if (!string.IsNullOrEmpty(_defaultValue))
                ConsoleHelper.Write(_helperText, ConsoleColor.Gray);

            Console.Write(value: " ");
        }
    }
}