// -----------------------------------------------------------------------
//  <copyright file="SwitchCliControl.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Controls
{
    using System;
    using Helpers;

    public class SwitchCliControl : CliControl<bool>
    {
        readonly string _text;
        readonly bool _defaultValue;

        public SwitchCliControl(string text, bool defaultValue)
        {
            _text = text;
            _defaultValue = defaultValue;
        }

        public override bool Run()
        {
            Write();
            bool? result = null;
            while (result == null)
            {
                var read = ConsoleHelper.Read();
                result = ProccessInput(read);
                var remoteLength = read.Length;
                for (int i = 0; i < remoteLength; i++)
                    Console.Write(value: "\b \b");
            }

            for (int i = 0; i < 6; i++)
                Console.Write(value: "\b \b");

            ConsoleHelper.Write(result.Value ? "Yes" : "No", ConsoleColor.DarkCyan);
            Console.WriteLine();
            return result.Value;
        }

        protected override void Write()
        {
            ConsoleHelper.Write(value: "? ", foreColor: ConsoleColor.DarkGreen);
            ConsoleHelper.Write(_text, ConsoleColor.White);
            if (_defaultValue)
                ConsoleHelper.Write(value: " (Y/n) ", foreColor: ConsoleColor.Gray);
            else
                ConsoleHelper.Write(value: " (y/N) ", foreColor: ConsoleColor.Gray);
        }

        bool? ProccessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return _defaultValue;

            if (input.Equals(value: "y", comparisonType: StringComparison.OrdinalIgnoreCase))
                return true;
            if (input.Equals(value: "n", comparisonType: StringComparison.OrdinalIgnoreCase))
                return false;

            return null;
        }
    }
}