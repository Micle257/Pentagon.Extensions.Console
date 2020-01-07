// -----------------------------------------------------------------------
//  <copyright file="ParseResultExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System;
    using System.CommandLine;
    using System.Linq;
    using JetBrains.Annotations;

    public static class ParseResultExtensions
    {
        public static T GetCommand<T>([NotNull] this ParseResult parseResult)
        {
            var info = CliCommandContext.Instance.CommandInfos.FirstOrDefault(a => a.Describer.Type == typeof(T));

            var commandResult = parseResult.FindResultFor(info.Command);

            if (commandResult != null)
            {
                var command = Activator.CreateInstance<T>();

                foreach (var cliOptionInfo in info.Options)
                {
                    var optionResult = parseResult.FindResultFor(cliOptionInfo.Option);

                    if (optionResult != null)
                    {
                        var value = parseResult.ValueForOption(cliOptionInfo.Option.RawAliases[0]);

                        cliOptionInfo.Describer.PropertyInfo.SetValue(command, value);
                    }
                }

                foreach (var cliArgumentInfo in info.Arguments)
                {
                    var argumentResult = parseResult.FindResultFor(cliArgumentInfo.Argument);

                    if (argumentResult != null)
                    {
                        var values = argumentResult.GetValueOrDefault();

                        cliArgumentInfo.Describer.PropertyInfo.SetValue(command, values);
                    }
                }

                return command;
            }

            return default;
        }
    }
}