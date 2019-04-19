// -----------------------------------------------------------------------
//  <copyright file="SwitchCliControl.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Controls
{
    using System;
    using System.Globalization;

    public class SwitchCliControl : CliControl<bool>
    {
        readonly static string YesName = Localization.Get(LocalizationKeyNames.Yes);
        readonly static string NoName = Localization.Get(LocalizationKeyNames.No);
        readonly static string YesShortName = Localization.Get(LocalizationKeyNames.YesShort);
        readonly static string NoShortName = Localization.Get(LocalizationKeyNames.NoShort);

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

            ConsoleHelper.Write(result.Value ? YesName : NoName, ConsoleColor.DarkCyan);
            Console.WriteLine();
            return result.Value;
        }

        protected override void Write()
        {
            ConsoleHelper.Write(value: "? ", foreColor: ConsoleColor.DarkGreen);
            ConsoleHelper.Write(_text, ConsoleColor.White);
            if (_defaultValue)
                ConsoleHelper.Write(value: $" ({YesShortName.ToUpper()}/{NoShortName}) ", foreColor: ConsoleColor.Gray);
            else
                ConsoleHelper.Write(value: $" ({YesShortName}/{NoShortName.ToUpper()}) ", foreColor: ConsoleColor.Gray);
        }

        bool? ProccessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return _defaultValue;

            if (input.Equals(value: YesShortName, comparisonType: StringComparison.OrdinalIgnoreCase))
                return true;
            if (input.Equals(value: NoShortName, comparisonType: StringComparison.OrdinalIgnoreCase))
                return false;

            return null;
        }
    }
}