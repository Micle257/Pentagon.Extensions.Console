// -----------------------------------------------------------------------
//  <copyright file="SwitchCliControl.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Controls
{
    using System;
    using Resources.Localization;

    public class SwitchCliControl : CliControl<bool>
    {
        static readonly string YesName = Localization.Get(LocalizationKeyNames.Yes);
        static readonly string NoName = Localization.Get(LocalizationKeyNames.No);
        static readonly string YesShortName = Localization.Get(LocalizationKeyNames.YesShort);
        static readonly string NoShortName = Localization.Get(LocalizationKeyNames.NoShort);
        static readonly string ErrorHeader = Localization.Get(LocalizationKeyNames.ErrorHeader);
        static readonly string Error = Localization.Get(LocalizationKeyNames.SwitchErrorContent);

        readonly string _text;
        readonly bool _defaultValue;
        bool _hasError;
        int _initialPosition;
        int _remoteLength;

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

            ConsoleWriter.Write(result.Value ? YesName : NoName, ConsoleColor.DarkCyan);
            Console.WriteLine();
            return result.Value;
        }

        protected override void Write()
        {
            Console.CursorTop = _initialPosition;
            Console.CursorLeft = 0;

            ConsoleWriter.Write(value: "? ", ConsoleColor.DarkGreen);
            ConsoleWriter.Write(_text, ConsoleColor.White);

            if (_defaultValue)
                ConsoleWriter.Write($" ({YesShortName.ToUpper()}/{NoShortName}) ", ConsoleColor.Gray);
            else
                ConsoleWriter.Write($" ({YesShortName}/{NoShortName.ToUpper()}) ", ConsoleColor.Gray);

            var readPosition = (Console.CursorTop, Console.CursorLeft);

            var errorHeader = ErrorHeader;
            var errorContent = Error;
            var errorLength = errorHeader.Length + errorContent.Length + 1;

            if (_hasError)
            {
                Console.WriteLine();
                ConsoleWriter.Write(errorHeader, ConsoleColor.Red);
                Console.Write(value: " ");
                ConsoleWriter.Write(errorContent);
            }
            else
            {
                Console.WriteLine();
                ConsoleWriter.Write(new string(' ', errorLength));
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
                return _defaultValue;

            if (input.Equals(YesShortName, StringComparison.OrdinalIgnoreCase))
                return true;
            if (input.Equals(NoShortName, StringComparison.OrdinalIgnoreCase))
                return false;

            _hasError = true;

            return null;
        }
    }
}