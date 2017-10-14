// -----------------------------------------------------------------------
//  <copyright file="Shell.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Helpers
{
    using System.Diagnostics;

    public class Shell
    {
        public static string Bash(string command)
        {
            var args = command.Replace(oldValue: "\"", newValue: "\\\"");
            return Run(fileName: "/bin/bash", arguments: $"-c \"{args}\"");
        }

        public static string Bat(string command)
        {
            var args = command.Replace(oldValue: "\"", newValue: "\\\"");
            return Run(fileName: "cmd.exe", arguments: $"/c \"{args}\"");
        }

        static string Run(string fileName, string arguments)
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

            process.Start();
            var processResult = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return processResult;
        }
    }
}