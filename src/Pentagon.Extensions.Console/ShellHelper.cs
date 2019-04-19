﻿// -----------------------------------------------------------------------
//  <copyright file="ShellHelper.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console
{
    using System;
    using System.Diagnostics;

    public class ShellHelper
    {
        public static string RunCommand(string command)
        {
            switch (OS.Platform)
            {
                case OperatingSystemPlatform.Windows:
                    return Batch(command);

                case OperatingSystemPlatform.Linux:
                case OperatingSystemPlatform.OSX:
                    return Bash(command);
            }

            return null;
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

        public static string Bash(string command)
        {
            var args = command.Replace(oldValue: "\"", newValue: "\\\"");
            return Run(GetBashFile(), arguments: $"-c \"{args}\"");
        }

        public static string Batch(string command)
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