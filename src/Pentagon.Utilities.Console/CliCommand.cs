﻿// -----------------------------------------------------------------------
//  <copyright file="CliCommand.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------
namespace Pentagon.Utilities.Console
{
    using System.Linq;
    using Helpers;
    using McMaster.Extensions.CommandLineUtils;

    [HelpOption(template: "--help|-h")]
    public abstract class CliCommand
    {
        protected virtual int OnExecute(CommandLineApplication app)
        {
            ProcessCode(0);
            app.ShowHelp();
            return 0;
        }

        protected virtual int ProcessCode(int returnCode)
        {
            switch (returnCode)
            {
                case 0:
                    break;

                default:
                    ConsoleHelper.WriteError(errorValue: "Invalid command");
                    break;
            }

            return returnCode;
        }
    }
}