// -----------------------------------------------------------------------
//  <copyright file="TextCliControl.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Controls
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public static class CliLabels
    {
        [NotNull]
        public static CliLabel FormField = new CliLabel
                                         {
                                                 Prefix = '?',
                                                 PrefixColor = new CliConsoleColor(ConsoleColor.DarkGreen),
                                                 DescriptionColor = ConsoleHelper.ColorScheme.Text
                                         };

        [NotNull]
        public static CliLabel Success = new CliLabel
                                         {
                                                 Prefix = '+',
                                                 PrefixColor = ConsoleHelper.ColorScheme.Success,
                                                 DescriptionColor = ConsoleHelper.ColorScheme.Text
                                         };

        [NotNull]
        public static CliLabel Info = new CliLabel
                                         {
                                                 Prefix = 'i',
                                                 PrefixColor = ConsoleHelper.ColorScheme.Info,
                                                 DescriptionColor = ConsoleHelper.ColorScheme.Text
                                         };

        [NotNull]
        public static CliLabel Error = new CliLabel
                                      {
                                              Prefix = 'x',
                                              PrefixColor = ConsoleHelper.ColorScheme.Error,
                                              DescriptionColor = ConsoleHelper.ColorScheme.Text
                                      };

        [NotNull]
        public static CliLabel Warning = new CliLabel
                                      {
                                              Prefix = '!',
                                              PrefixColor = ConsoleHelper.ColorScheme.Warning,
                                              DescriptionColor = ConsoleHelper.ColorScheme.Text
                                      };
    }

    public class CliLabel
    {
        public char? Prefix { get; set; }

        public CliConsoleColor PrefixColor { get; set; }

        public CliConsoleColor DescriptionColor { get; set; }

        public void Write(string description)
        {
            ConsoleHelper.EnsureNewLine();

            if (Prefix.HasValue)
            {
                ConsoleHelper.Write(Prefix.Value, PrefixColor);

                Console.Write(new string(' ', 1));
            }

            if (description != null)
            {
                ConsoleHelper.Write(description, DescriptionColor);
            }
        }
    }

    public class TextCliControl : CliControl<string>
    {
        string _text;
        string _defaultValue;
        string _helperText;
        string _typedText;

        public string Text
        {
            get => _text;
            set => _text = value;
        }

        public string DefaultValue
        {
            get => _defaultValue;
            set
            {
                _defaultValue = value ?? string.Empty;
                _helperText = value != null ? $" ({_defaultValue})" : "";
            }
        }

        public string TypedText
        {
            get => _typedText;
            set => _typedText = value;
        }

        public CliLabel Label { get; set; }

        public override string Run()
        {
            Write();

            var read = ConsoleHelper.Read(_typedText);

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
            (Label ?? CliLabels.FormField).Write(_text);

            if (!string.IsNullOrEmpty(_defaultValue))
                ConsoleHelper.Write(_helperText, ConsoleColor.Gray);

            Console.Write(value: " ");
        }
    }
}