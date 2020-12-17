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
    using System.Reflection;
    using JetBrains.Annotations;

    public static class ParseResultExtensions
    {
        public static T GetCommand<T>([NotNull] this ParseResult parseResult)
        {
            var info = CliCommandCompileContext.Instance.CommandInfos.FirstOrDefault(a => a.Describer.Type == typeof(T));

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

                        SetValue(cliOptionInfo.Describer.PropertyInfo, command, value);
                    }
                }

                foreach (var cliArgumentInfo in info.Arguments)
                {
                    var argumentResult = parseResult.FindResultFor(cliArgumentInfo.Argument);

                    if (argumentResult != null)
                    {
                        var values = argumentResult.GetValueOrDefault();

                        SetValue(cliArgumentInfo.Describer.PropertyInfo, command, values);
                    }
                }

                return command;
            }

            return default;

            void SetValue(MemberInfo cliOptionInfo, object command, object value)
            {
                if (cliOptionInfo is FieldInfo field)
                    field.SetValue(command, value);
                else if (cliOptionInfo is PropertyInfo prop)
                    prop.SetValue(command, value);
                else
                    throw new ArgumentException($"Invalid member: {cliOptionInfo}");
            }
        }
    }
}