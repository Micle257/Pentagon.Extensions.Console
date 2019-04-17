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
    using Controls;

    public static class ConsoleHelper
    {
        public static void Write(object value, ConsoleColor? foreColor, ConsoleColor? backColor = null)
        {
            var fore = Console.ForegroundColor;
            var back = Console.BackgroundColor;
            if (foreColor.HasValue)
                Console.ForegroundColor = foreColor.Value;
            if (backColor.HasValue)
                Console.BackgroundColor = backColor.Value;
            Console.Write(value);
            if (foreColor.HasValue)
                Console.ForegroundColor = fore;
            if (backColor.HasValue)
                Console.BackgroundColor = back;
        }

        public static void WriteLine(object value, ConsoleColor? foreColor, ConsoleColor? backColor = null)
        {
            var fore = Console.ForegroundColor;
            var back = Console.BackgroundColor;
            if (foreColor.HasValue)
                Console.ForegroundColor = foreColor.Value;
            if (backColor.HasValue)
                Console.BackgroundColor = backColor.Value;
            Console.WriteLine(value);
            if (foreColor.HasValue)
                Console.ForegroundColor = fore;
            if (backColor.HasValue)
                Console.BackgroundColor = back;
        }

        public static void WriteSuccess(object successValue) => Write(successValue, ConsoleColor.Green);

        public static void WriteError(object errorValue) => Write(errorValue, ConsoleColor.Red);

        public static void WriteWarning(object warningValue) => Write(warningValue, ConsoleColor.Yellow);

        public static SecureString ReadSecret(SecretTextOutputMode outputMode = SecretTextOutputMode.NoOutput)
        {
            var secret = new SecureString();

            var writeAsterisk = outputMode == SecretTextOutputMode.Asterisk;
            var peekLast = outputMode == SecretTextOutputMode.PeekLast;

            while (true)
            {
                var i = Console.ReadKey(true);

                if (peekLast && secret.Length > 0)
                {
                    Console.Write(value: "\b*");
                }

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

        public static string Read()
        {
            var result = new StringBuilder();
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
    }
}