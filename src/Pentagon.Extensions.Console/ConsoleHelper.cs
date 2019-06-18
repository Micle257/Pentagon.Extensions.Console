// -----------------------------------------------------------------------
//  <copyright file="ConsoleHelper.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console
{
    using System;
    using System.Security;
    using System.Text;
    using Commands;
    using Controls;
    using JetBrains.Annotations;

    public static class ConsoleHelper
    {
        public static void ColoredWrite(Action action, CliConsoleColor color)
        {
            var fore = Console.ForegroundColor;
            var back = Console.BackgroundColor;

            if (color.Foreground.HasValue)
                Console.ForegroundColor = color.Foreground.Value;
            if (color.Background.HasValue)
                Console.BackgroundColor = color.Background.Value;

            action?.Invoke();

            if (color.Foreground.HasValue)
                Console.ForegroundColor = fore;
            if (color.Background.HasValue)
                Console.BackgroundColor = back;
        }

        public static void ColoredWrite(Action action, ConsoleColor foreColor, ConsoleColor backColor)
        {
            ColoredWrite(action, new CliConsoleColor(foreColor, backColor));
        }

        public static void ColoredWrite(Action action, ConsoleColor foreColor)
        {
            ColoredWrite(action, new CliConsoleColor(foreColor));
        }

        public static void Write(object value, CliConsoleColor color)
        {
            ColoredWrite(() => Console.Write(value), color);
        }

        public static void Write(object value, ConsoleColor? foreColor = null, ConsoleColor? backColor = null)
        {
            Write(value, new CliConsoleColor(foreColor, backColor));
        }

        public static void WriteLine(object value, CliConsoleColor color)
        {
            ColoredWrite(() => Console.WriteLine(value), color);
        }

        public static void WriteLine(object value, ConsoleColor? foreColor = null, ConsoleColor? backColor = null)
        {
            WriteLine(value, new CliConsoleColor(foreColor, backColor));
        }

        public static void WriteSuccess(object successValue) => Write(successValue, Cli.ColorScheme.Success);

        public static void WriteError(object errorValue) => Write(errorValue, Cli.ColorScheme.Error);

        public static void WriteWarning(object warningValue) => Write(warningValue, Cli.ColorScheme.Warning);

        public static SecureString ReadSecret(SecretTextOutputMode outputMode = SecretTextOutputMode.NoOutput)
        {
            var secret = new SecureString();

            var writeAsterisk = outputMode == SecretTextOutputMode.Asterisk;
            var peekLast = outputMode == SecretTextOutputMode.PeekLast;

            while (true)
            {
                var i = Console.ReadKey(true);

                if (peekLast && secret.Length > 0)
                    Console.Write(value: "\b*");

                if (i.Key == ConsoleKey.Enter)
                    break;
                if (i.Key == ConsoleKey.Backspace)
                {
                    if (secret.Length > 0 && (writeAsterisk || peekLast))
                    {
                        secret.RemoveAt(secret.Length - 1);
                        Console.Write(value: "\b \b");
                    }
                }
                else if (!char.IsControl(i.KeyChar))
                {
                    secret.AppendChar(i.KeyChar);

                    if (writeAsterisk)
                        Console.Write(value: "*");
                    else if (peekLast)
                        Console.Write(i.KeyChar);
                }
            }

            return secret;
        }

        [NotNull]
        public static string Read(string prefix = null)
        {
            var result = new StringBuilder();

            if (prefix != null)
            {
                Console.Write(prefix);
                result.Append(prefix);
            }

            while (true)
            {
                var i = Console.ReadKey(true);

                if (i.Key == ConsoleKey.Enter)
                    break;
                if (i.Key == ConsoleKey.Backspace)
                {
                    if (result.Length > 0)
                    {
                        result.Remove(result.Length - 1, 1);
                        Console.Write(value: "\b \b");
                    }
                }
                else
                {
                    result.Append(i.KeyChar);
                    Console.Write(i.KeyChar);
                }
            }

            return result.ToString();
        }

        public static void PlayWavFile(string wavFilePath, bool writeOutput = false)
        {
            switch (OS.Platform)
            {
                case OperatingSystemPlatform.Linux:
                    var output = ShellHelper.RunCommand($"aplay {wavFilePath}");
                    if (writeOutput)
                        Console.WriteLine(output);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        public static void EnsureNewLine()
        {
            if (Console.CursorLeft > 0)
            {
                Console.CursorLeft = 0;
                Console.CursorTop++;
            }
        }

        public static void WriteSpace(int count = 1)
        {
            if (count >= 1)
            {
                Write(new string(' ',count), Cli.ColorScheme.Blank);
            }
        }
    }
}