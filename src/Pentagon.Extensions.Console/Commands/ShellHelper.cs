// -----------------------------------------------------------------------
//  <copyright file="ShellHelper.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Commands
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public class ShellHelper
    {
        public static CommandResult RunCommand(string command)
        {
            return RunCommandAsync(command).Result;
        }

        public static Task<CommandResult> RunCommandAsync(string command, CancellationToken cancellationToken = default)
        {
            switch (OS.Platform)
            {
                case OperatingSystemPlatform.Windows:
                    return BatchAsync(command, cancellationToken);

                case OperatingSystemPlatform.Linux:
                case OperatingSystemPlatform.OSX:
                    return BatchAsync(command, cancellationToken);
            }

            return Task.FromResult(new CommandResult
                                   {
                                           Exception = new NotSupportedException("OS not supported.")
                                   });
        }

        static string GetBashFile()
        {
            switch (OS.Platform)
            {
                case OperatingSystemPlatform.Windows:
                    return "C:\\Program Files\\Git\\bin\\bash.exe";

                case OperatingSystemPlatform.Linux:
                case OperatingSystemPlatform.OSX:
                    return "/bin/bash";
            }

            throw new NotSupportedException();
        }

        public static CommandResult Bash(string command)
        {
            return BashAsync(command).Result;
        }

        public static CommandResult Batch(string command)
        {
            return BatchAsync(command).Result;
        }

        public static Task<CommandResult> BashAsync(string command, CancellationToken cancellationToken = default)
        {
            var args = command.Replace(oldValue: "\"", newValue: "\\\"");
            return RunAsync(GetBashFile(), arguments: $"-c \"{args}\"", cancellationToken: cancellationToken);
        }

        public static Task<CommandResult> BatchAsync(string command, CancellationToken cancellationToken = default)
        {
            var args = command.Replace(oldValue: "\"", newValue: "\\\"");
            return RunAsync(fileName: "cmd.exe", arguments: $"/c \"{args}\"", cancellationToken: cancellationToken);
        }

        static async Task<CommandResult> RunAsync(string fileName, string arguments, CancellationToken cancellationToken = default)
        {
            var process = new Process
                          {
                                  StartInfo = new ProcessStartInfo
                                              {
                                                      FileName = fileName,
                                                      Arguments = arguments,
                                                      RedirectStandardOutput = true,
                                                      UseShellExecute = false,
                                                      CreateNoWindow = false
                                              }
                          };

            await process.StartAndWaitAsync(cancellationToken);

            var processResult = process.StandardOutput.ReadToEnd();

            if (process.ExitCode != 0)
            {
                return new CommandResult
                                 {
                                         Process = process,
                                         Content = processResult,
                                         Exception = new CommandFailedException(fileName, arguments, process.StandardError.ReadToEnd(), process.ExitCode)
                                 };
            }

            return new CommandResult
                             {
                                     Process = process,
                                     Content = processResult
                             };
        }
    }
}