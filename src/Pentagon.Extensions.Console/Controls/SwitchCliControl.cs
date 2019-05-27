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
        readonly static string ErrorHeader = Localization.Get(LocalizationKeyNames.ErrorHeader);
        readonly static string Error = Localization.Get(LocalizationKeyNames.SwitchErrorContent);

        readonly string _text;
        readonly bool _defaultValue;
        bool _hasError = false;
        int _initialPosition;
        int _remoteLength = 0;

        public SwitchCliControl(string text, bool defaultValue)
        {
            _text = text;
            _defaultValue = defaultValue;
        }

        public override bool Run()
        {
            _initialPosition = Console.CursorTop;

            Write();
            bool? result = null;
            while (result == null)
            {
                var read = ConsoleHelper.Read();

                _remoteLength = read.Length;

                result = ProccessInput(read);

                Write();

               // for (var i = 0; i < remoteLength; i++)
               //     Console.Write(value: "\b \b");
            }

            for (var i = 0; i < 6; i++)
                Console.Write(value: "\b \b");

            ConsoleHelper.Write(result.Value ? YesName : NoName, ConsoleColor.DarkCyan);
            Console.WriteLine();
            return result.Value;
        }

        protected override void Write()
        {
            Console.CursorTop = _initialPosition;
            Console.CursorLeft = 0;

            ConsoleHelper.Write(value: "? ", foreColor: ConsoleColor.DarkGreen);
            ConsoleHelper.Write(_text, ConsoleColor.White);

            if (_defaultValue)
                ConsoleHelper.Write(value: $" ({YesShortName.ToUpper()}/{NoShortName}) ", foreColor: ConsoleColor.Gray);
            else
                ConsoleHelper.Write(value: $" ({YesShortName}/{NoShortName.ToUpper()}) ", foreColor: ConsoleColor.Gray);

            var readPosition = (Console.CursorTop, Console.CursorLeft);

            var errorHeader = ErrorHeader;
            var errorContent = Error;
            var errorLength = errorHeader.Length + errorContent.Length + 1;

            if (_hasError)
            {
                Console.WriteLine();
                ConsoleHelper.Write(errorHeader, ConsoleColor.Red);
                Console.Write(" ");
                ConsoleHelper.Write(errorContent);
            }
            else
            {
                Console.WriteLine();
                ConsoleHelper.Write(new string(' ', errorLength));
            }

            Console.CursorTop = readPosition.CursorTop;
            Console.CursorLeft = readPosition.CursorLeft;
        }

        bool? ProccessInput(string input)
        {
            for (var i = 0; i < _remoteLength; i++)
                Console.Write(value: "\b \b");

            _hasError = false;

            if (string.IsNullOrWhiteSpace(input))
            {
                return _defaultValue;
            }

            if (input.Equals(value: YesShortName, comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (input.Equals(value: NoShortName, comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            _hasError = true;

            return null;
        }
    }
}