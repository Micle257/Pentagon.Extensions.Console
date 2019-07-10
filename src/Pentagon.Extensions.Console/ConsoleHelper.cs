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
                ConsoleWriter.Write(new string(' ',count), Cli.ColorScheme.Blank);
            }
        }
    }
}