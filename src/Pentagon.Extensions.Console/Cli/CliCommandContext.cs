// -----------------------------------------------------------------------
//  <copyright file="CliCommandContext.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Collections.Tree;
    using Helpers;
    using JetBrains.Annotations;

    public class CliCommandContext : ICliCommandContext
    {
        IReadOnlyList<CliCommandInfo> _commandInfoCache;

        HierarchyList<CliCommandDescriber> _commandHierarchy;

        IReadOnlyList<CliCommandDescriber> _commandDescriberCache;

        [NotNull]
        public static CliCommandContext Instance { get; } = new CliCommandContext();

        public CliRootCommandInfo RootCommandInfo => (CliRootCommandInfo)CommandInfos.Single(a => a is CliRootCommandInfo);

        public IReadOnlyList<CliCommandInfo> CommandInfos => _commandInfoCache ?? (_commandInfoCache = GetCommandInfos().ToList().AsReadOnly());

        public HierarchyList<CliCommandDescriber> CommandHierarchy => _commandHierarchy ?? (_commandHierarchy = GetCommandHierarchy());

        public CliCommandDescriber RootCommandDescriber => CommandDescribers.Single(a => a.IsRoot);

        public IReadOnlyList<CliCommandDescriber> CommandDescribers =>
                _commandDescriberCache ?? (_commandDescriberCache = GetCommandDescribers().Prepend(GetRootCommandDescriber()).Distinct().ToList().AsReadOnly());

        [Pure]
        [NotNull]
        static CliCommandDescriber GetRootCommandDescriber()
        {
            var rootCommands = AppDomain.CurrentDomain
                                        .GetLoadedTypes()
                                        .Where(a => a.GetCustomAttribute<CliRootCommandAttribute>(false) != null).ToList();

            if (rootCommands.Count == 0)
                throw new Exception("No root commands found.");

            if (rootCommands.Count > 1)
            {
                if (rootCommands.Count == 2 && rootCommands.Contains(typeof(CliRootCommand)))
                    rootCommands.Remove(typeof(CliRootCommand));
                else
                    throw new Exception("More than one root command found.");
            }

            var rootCommandType = rootCommands.First();

            var rootCommandAttribute = rootCommandType.GetCustomAttribute<CliRootCommandAttribute>();

            var options = GetOptions(rootCommandType);
            var arguments = GetArguments(rootCommandType);

            var info = new CliCommandDescriber(rootCommandType, rootCommandAttribute, options, arguments);

            return info;
        }

        [Pure]
        [NotNull]
        [ItemNotNull]
        static IEnumerable<CliCommandDescriber> GetCommandDescribers()
        {
            var commands = AppDomain.CurrentDomain
                                    .GetLoadedTypes()
                                    .Where(a => a.GetCustomAttribute<CliCommandAttribute>(false) != null).ToList();

            foreach (var type in commands)
            {
                var attribute = type.GetCustomAttribute<CliCommandAttribute>();

                var options = GetOptions(type).ToList();

                var arguments = GetArguments(type).ToList();

                var cliCommandInfo = new CliCommandDescriber(type, attribute, options, arguments);

                yield return cliCommandInfo;
            }
        }

        [Pure]
        [NotNull]
        [ItemNotNull]
        static IEnumerable<Type> GetCommandSubCommandTypes([NotNull] CliCommandDescriber info)
        {
            var processed = new HashSet<Type>();

            foreach (var type in AppDomain.CurrentDomain.GetLoadedTypes()
                                          .Where(t => t.BaseType == info.Type))
            {
                if (!processed.Contains(type))
                    yield return type;

                processed.Add(type);
            }

            var attributes = info.Type.GetCustomAttributes<SubCliCommandAttribute>();

            foreach (var subCliCommandAttribute in attributes)
            {
                if (!processed.Contains(subCliCommandAttribute.Type))
                    yield return subCliCommandAttribute.Type;

                processed.Add(subCliCommandAttribute.Type);
            }

            if (info.Type == typeof(CliRootCommand))
            {
                foreach (var type in AppDomain.CurrentDomain.GetLoadedTypes()
                                              .Where(a => a != info.Type && a.BaseType == typeof(object) && a.GetCustomAttribute<CliCommandAttribute>(false) != null))
                {
                    if (!processed.Contains(type))
                        yield return type;

                    processed.Add(type);
                }
            }
        }

        [Pure]
        [NotNull]
        static Option GetOption([NotNull] CliOptionDescriber describer)
        {
            var aliases = describer.Attribute.Aliases;

            if (aliases.Count == 0)
                aliases = new[] { "--" + Regex.Replace(describer.PropertyInfo.Name, "([A-Z])([a-z]+)", a => a.Groups[1].Value.ToLower() + a.Groups[2].Value + "-").TrimEnd('-') };

            var name = string.IsNullOrWhiteSpace(describer.Attribute.Name) ? describer.PropertyInfo.Name : describer.Attribute.Name;

            var options = new Option(aliases.ToArray())
            {
                Required = describer.Attribute.IsRequired,
                IsHidden = describer.Attribute.IsHidden,
                Name = name,
                Argument = new Argument { ArgumentType = describer.PropertyInfo.PropertyType },
                Description = describer.Attribute.Description
            };

            return options;
        }

        [Pure]
        [NotNull]
        [ItemNotNull]
        static IEnumerable<CliOptionDescriber> GetOptions([NotNull] Type type)
        {
            var autoProperties = type.GetAutoProperties()
                                     .Select(a => (a, a.GetCustomAttribute<CliOptionAttribute>()))
                                     .Where(a => a.Item2 != null)
                                     .ToList();

            foreach (var (property, attribute) in autoProperties)
                yield return new CliOptionDescriber(property, attribute);
        }

        [Pure]
        [NotNull]
        static Argument GetArgument([NotNull] CliArgumentDescriber describer)
        {
            var name = string.IsNullOrWhiteSpace(describer.Attribute.Name) ? describer.PropertyInfo.Name : describer.Attribute.Name;

            var minimumNumberOfValues = describer.Attribute.MinumumNumberOfValuesOverride != -1
            ? describer.Attribute.MinumumNumberOfValuesOverride
                    : describer.Attribute.IsRequired ? 1 : 0;

            var arg = new Argument
            {
                ArgumentType = describer.PropertyInfo.PropertyType,
                Name = name,
                Description = describer.Attribute.Description,
                IsHidden = describer.Attribute.IsHidden,
                Arity = new ArgumentArity(minimumNumberOfValues, describer.Attribute.MaximumNumberOfValues)
            };

            return arg;
        }

        [Pure]
        [NotNull]
        [ItemNotNull]
        static IEnumerable<CliArgumentDescriber> GetArguments([NotNull] Type type)
        {
            var autoProperties = type.GetAutoProperties()
                                     .Select(a => (a, a.GetCustomAttribute<CliArgumentAttribute>()))
                                     .Where(a => a.Item2 != null)
                                     .ToList();

            foreach (var (property, attribute) in autoProperties)
            {
                if (property.PropertyType == typeof(string) && attribute.MaximumNumberOfValues == 1
                 || typeof(IEnumerable<string>).IsAssignableFrom(property.PropertyType) && attribute.MaximumNumberOfValues >= 1)
                    yield return new CliArgumentDescriber(property, attribute);
                else
                    throw new InvalidOperationException($"Invalid type: {property.PropertyType} for argument.");
            }
        }

        [Pure]
        [ItemNotNull]
        [NotNull]
        IEnumerable<CliCommandInfo> GetCommandInfos()
        {
            var commands = new Dictionary<Type, Command>();

            foreach (var node in CommandHierarchy.OrderByDescending(a => a.Depth))
            {
                var commandInfo = node.Value;

                Command command;

                if (node.IsRoot)
                {
                    var rootCommand = new RootCommand(commandInfo.Attribute.Description);

                    if (!string.IsNullOrWhiteSpace(commandInfo.Attribute.Name))
                        rootCommand.Name = commandInfo.Attribute.Name;

                    command = rootCommand;
                }
                else
                    command = new Command(commandInfo.Attribute.Name ?? commandInfo.Type.Name, commandInfo.Attribute.Description);

                if (commandInfo.Attribute.AlternedNames?.Any() == true)
                {
                    foreach (var s in commandInfo.Attribute.AlternedNames)
                        command.AddAlias(s);
                }

                var options = new List<CliOptionInfo>();
                var arguments = new List<CliArgumentInfo>();

                foreach (var optionInfo in commandInfo.Options)
                {
                    var option = GetOption(optionInfo);

                    command.AddOption(option);

                    options.Add(new CliOptionInfo(option, optionInfo));
                }

                foreach (var argumentInfo in commandInfo.Arguments.OrderBy(a => a.Attribute.Order))
                {
                    var argument = GetArgument(argumentInfo);

                    command.AddArgument(argument);

                    arguments.Add(new CliArgumentInfo(argument, argumentInfo));
                }

                commands.Add(commandInfo.Type, command);

                // if command has sub commands...
                if (node.IsBranchNode())
                {
                    foreach (var subCommandInfo in node.Children.Select(a => a.Value))
                    {
                        var inDict = commands[subCommandInfo.Type];

                        command.AddCommand(inDict);
                    }
                }

                if (node.IsRoot)
                    yield return new CliRootCommandInfo((RootCommand)command, commandInfo, options, arguments);
                else
                    yield return new CliCommandInfo(command, commandInfo, options, arguments);
            }
        }

        [Pure]
        [NotNull]
        [ItemNotNull]
        HierarchyList<CliCommandDescriber> GetCommandHierarchy()
        {
            var commandSubMap = CommandDescribers.Select(a => (a, GetCommandSubCommands(a).ToList())).ToDictionary(a => a.Item1, a => a.Item2);

            var hierarchy = HierarchyList<CliCommandDescriber>.FromDictionaryFreely(RootCommandDescriber, commandSubMap);

            return hierarchy;
        }

        [Pure]
        [ItemNotNull]
        [NotNull]
        IEnumerable<CliCommandDescriber> GetCommandSubCommands([NotNull] CliCommandDescriber info)
        {
            foreach (var commandType in GetCommandSubCommandTypes(info))
            {
                var subInfo = CommandDescribers.Single(a => a.Type == commandType);

                yield return subInfo;
            }
        }
    }
}