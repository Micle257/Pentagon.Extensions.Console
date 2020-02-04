// -----------------------------------------------------------------------
//  <copyright file="ShellHelper.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Commands
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Threading;

    public class ShellHelper
    {
        public static string BashLocation { get; set; }

        public static CommandResult RunCommand(string command, string workingDirectory = null) => RunCommandAsync(command, workingDirectory).AwaitSynchronously();

        public static Task<CommandResult> RunCommandAsync(string command, string workingDirectory = null, CancellationToken cancellationToken = default)
        {
            switch (OS.Platform)
            {
                case OperatingSystemPlatform.Windows:
                    return BatchAsync(command, workingDirectory, cancellationToken);

                case OperatingSystemPlatform.Linux:
                case OperatingSystemPlatform.OSX:
                    return BatchAsync(command, workingDirectory, cancellationToken);
            }

            return Task.FromResult(new CommandResult
                                   {
                                           Exception = new NotSupportedException(message: "OS not supported.")
                                   });
        }

        public static CommandResult Bash(string command, string workingDirectory = null) => BashAsync(command, workingDirectory).AwaitSynchronously();

        public static CommandResult Batch(string command, string workingDirectory = null) => BatchAsync(command, workingDirectory).AwaitSynchronously();

        public static Task<CommandResult> BashAsync(string command, string workingDirectory = null, CancellationToken cancellationToken = default)
        {
            var args = command.Replace(oldValue: "\"", newValue: "\\\"");

            return RunAsync(GetBashFile(), $"-c \"{args}\"",workingDirectory, cancellationToken);
        }

        public static Task<CommandResult> BatchAsync(string command, string workingDirectory = null, CancellationToken cancellationToken = default)
        {
            var args = command.Replace(oldValue: "\"", newValue: "\\\"");
            return RunAsync(fileName: "cmd.exe", $"/c \"{args}\"",workingDirectory, cancellationToken);
        }

        static string GetBashFile()
        {
            switch (OS.Platform)
            {
                case OperatingSystemPlatform.Windows:
                    return BashLocation ?? "C:\\Program Files\\Git\\bin\\bash.exe";

                case OperatingSystemPlatform.Linux:
                case OperatingSystemPlatform.OSX:
                    return BashLocation ?? "/bin/bash";
            }

            throw new NotSupportedException();
        }

        static async Task<CommandResult> RunAsync(string fileName, string arguments, string workingDirectory = null, CancellationToken cancellationToken = default)
        {
            var process = new Process
                          {
                                  StartInfo = new ProcessStartInfo
                                              {
                                                      FileName = fileName,
                                                      Arguments = arguments,
                                                      WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
                                                      RedirectStandardOutput = true,
                                                      RedirectStandardError =  true,
                                                      UseShellExecute = false
                                              }
                          };

            await process.StartAndWaitAsync(cancellationToken).ConfigureAwait(false);

            var processResult = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
            var errorMsg = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                return new CommandResult
                       {
                               Process = process,
                               Content = processResult,
                               ErrorMessage = errorMsg,
                               Exception = new CommandFailedException(fileName, arguments, errorMsg, process.ExitCode)
                       };
            }

            return new CommandResult
                   {
                           Process = process,
                           ErrorMessage = errorMsg,
                            Content = processResult
                   };
        }
    }
}