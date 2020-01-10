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
    using System.CommandLine.Builder;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Builders;
    using Collections.Tree;
    using Helpers;
    using JetBrains.Annotations;

    public class CliCommandCompileContext : ICliCommandContext
    {
        IReadOnlyList<CliCommandInfo> _commandInfoCache;

        HierarchyList<CliCommandDescriber> _commandHierarchy;

        IReadOnlyList<CliCommandDescriber> _commandDescriberCache;

        [NotNull]
        public static CliCommandCompileContext Instance { get; } = new CliCommandCompileContext();

        public CliRootCommandInfo RootCommandInfo => (CliRootCommandInfo)CommandInfos.Single(a => a is CliRootCommandInfo);

        public IReadOnlyList<CliCommandInfo> CommandInfos => _commandInfoCache ??= GetCommandInfos().ToList().AsReadOnly();

        public HierarchyList<CliCommandDescriber> CommandHierarchy => _commandHierarchy ??= GetCommandHierarchy();

        public CliCommandDescriber RootCommandDescriber => CommandDescribers.Single(a => a.IsRoot);

        public IReadOnlyList<CliCommandDescriber> CommandDescribers =>
                _commandDescriberCache ??= GetAllCommandDescribers().Concat(_additionalDescribers).Distinct().ToList().AsReadOnly();

        [Pure]
        [NotNull]
        [ItemNotNull]
        IEnumerable<CliCommandDescriber> GetAllCommandDescribers()
        {
            var builder = new CliBuilder();

            yield return GetRootCommandDescriber(builder);

            foreach (var cliCommandDescriber in GetCommandDescribers(builder))
            {
                yield return cliCommandDescriber;
            }
        }

        [Pure]
        [NotNull]
        static CliCommandDescriber GetRootCommandDescriber(ICliBuilder builder)
        {
            var rootCommands = AppDomain.CurrentDomain
                                        .GetLoadedTypes()
                                        .Where(a => a.GetCustomAttribute<CliRootCommandAttribute>(false) != null).ToList();

            if (rootCommands.Count == 0)
            {
                var b = (CliCommandBuilder)builder.HasCommand(null);

                b.IsRoot();

                var rootInfo = b.Build();

                return rootInfo;

                throw new Exception("No root commands found.");
            }

            if (rootCommands.Count > 1)
            {
                throw new Exception("More than one root command found.");
            }

            var rootCommandType = rootCommands.First();

            var commandBuilder = (CliCommandBuilder)builder.HasCommand(rootCommandType);

            var rootCommandAttribute = rootCommandType.GetCustomAttribute<CliRootCommandAttribute>();

            commandBuilder.HasAttribute(rootCommandAttribute)
                          .IsRoot();

            HasOptions(rootCommandType, commandBuilder);
            HasArguments(rootCommandType, commandBuilder);

            var info = commandBuilder.Build();

            return info;
        }

        [Pure]
        [NotNull]
        [ItemNotNull]
        static IEnumerable<CliCommandDescriber> GetCommandDescribers(ICliBuilder builder)
        {
            var commands = AppDomain.CurrentDomain
                                    .GetLoadedTypes()
                                    .Where(a => a.GetCustomAttribute<CliCommandAttribute>(false) != null).ToList();

            foreach (var type in commands)
            {
                var commandBuilder = (CliCommandBuilder)builder.HasCommand(type);

                var attribute = type.GetCustomAttribute<CliCommandAttribute>();

                commandBuilder.HasAttribute(attribute);

                HasOptions(type, commandBuilder);
                HasArguments(type, commandBuilder);

                var cliCommandInfo = commandBuilder.Build();

                yield return cliCommandInfo;
            }
        }

        [Pure]
        [NotNull]
        static Option GetOption([NotNull] CliOptionDescriber describer)
        {
            var aliases = describer.Attribute.Aliases;

            if (aliases == null || aliases.Count == 0)
                aliases = new[] { "--" + Regex.Replace(describer.PropertyInfo.Name, "([A-Z])([a-z]+)", a => a.Groups[1].Value.ToLower() + a.Groups[2].Value + "-").TrimEnd('-') };

            var name = string.IsNullOrWhiteSpace(describer.Attribute.Name) ? describer.PropertyInfo.Name : describer.Attribute.Name;

            var options = new Option(aliases.ToArray())
            {
                Required = describer.Attribute.IsRequired,
                IsHidden = describer.Attribute.IsHidden,
                Name = name,
                Argument = new Argument { ArgumentType = describer.PropertyInfo.DeclaringType },
                Description = describer.Attribute.Description
            };

            return options;
        }

        static void HasOptions([NotNull] Type type, [NotNull] CliCommandBuilder commandBuilder)
        {
            var autoProperties = type.GetAutoProperties()
                                     .Select(a => (a, a.GetCustomAttribute<CliOptionAttribute>()))
                                     .Where(a => a.Item2 != null)
                                     .ToList();


            foreach (var (property, attribute) in autoProperties)
                commandBuilder.HasOption(property.Name, attribute);
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
                ArgumentType = describer.PropertyInfo.DeclaringType,
                Name = name,
                Description = describer.Attribute.Description,
                IsHidden = describer.Attribute.IsHidden,
                Arity = new ArgumentArity(minimumNumberOfValues, describer.Attribute.MaximumNumberOfValues)
            };

            return arg;
        }

        static void HasArguments([NotNull] Type type, [NotNull] CliCommandBuilder commandBuilder)
        {
            var autoProperties = type.GetAutoProperties()
                                     .Select(a => (a, a.GetCustomAttribute<CliArgumentAttribute>()))
                                     .Where(a => a.Item2 != null)
                                     .ToList();

            foreach (var (property, attribute) in autoProperties)
            {
                if (property.PropertyType == typeof(string) && attribute.MaximumNumberOfValues == 1
                 || typeof(IEnumerable<string>).IsAssignableFrom(property.PropertyType) && attribute.MaximumNumberOfValues >= 1)
                    commandBuilder.HasArgument(property.Name, attribute);
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

                    rootCommand.AddValidator(result => result.ValueForOption<bool>("--dry-run") ? "WHY?" : null);

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

                // type is null only if root type is specified implicitly
                if (commandInfo.Type != null)
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
            return FromStatic().Concat(FromParent()).Distinct();

            IEnumerable<CliCommandDescriber> FromParent()
            {
                var any = CommandDescribers.FirstOrDefault(a => a.Attribute.ParentType == (info.Type));

                if (any != null)
                {
                    yield return any;
                }
            }

            IEnumerable<CliCommandDescriber> FromStatic()
            {
                foreach (var commandType in GetCommandSubCommandTypes(info))
                {
                    var subInfo = CommandDescribers.Single(a => a.Type == commandType);

                    yield return subInfo;
                }
            }
        }

        [Pure]
        [NotNull]
        [ItemNotNull]
        static IEnumerable<Type> GetCommandSubCommandTypes([NotNull] CliCommandDescriber info)
        {
            if (info.Type == null)
            {
                foreach (var type in AppDomain.CurrentDomain.GetLoadedTypes()
                                              .Where(t => t.GetCustomAttribute<CliCommandAttribute>() != null && t.BaseType == typeof(object)))
                {
                    yield return type;
                }

                yield break;
            }

            var processed = new HashSet<Type>();

            // inheritance
            foreach (var type in AppDomain.CurrentDomain.GetLoadedTypes()
                                          .Where(t => t.BaseType == info.Type))
            {
                if (!processed.Contains(type))
                    yield return type;

                processed.Add(type);
            }

            var attributes = info.Type.GetCustomAttributes<SubCliCommandAttribute>();

            // sub command attribute
            foreach (var subCliCommandAttribute in attributes)
            {
                if (!processed.Contains(subCliCommandAttribute.Type))
                    yield return subCliCommandAttribute.Type;

                processed.Add(subCliCommandAttribute.Type);
            }

            var attribute = info.Attribute;

            // command attribute property
            if (attribute?.SubTypes != null && attribute.SubTypes.Length > 0)
            {
                foreach (var attributeSubType in attribute.SubTypes)
                {
                    if (!processed.Contains(attributeSubType))
                        yield return attributeSubType;

                    processed.Add(attributeSubType);
                }
            }

            //if (info.Type == typeof(CliRootCommand))
            //{
            //    foreach (var type in AppDomain.CurrentDomain.GetLoadedTypes()
            //                                  .Where(a => a != info.Type && a.BaseType == typeof(object) && a.GetCustomAttribute<CliCommandAttribute>(false) != null))
            //    {
            //        if (!processed.Contains(type))
            //            yield return type;
            //
            //        processed.Add(type);
            //    }
            //}
        }

        [NotNull]
        List<CliCommandDescriber> _additionalDescribers = new List<CliCommandDescriber>();

        internal void AddDescribers(IEnumerable<CliCommandDescriber> cliCommandDescribers)
        {
            // clear info cache
            _commandInfoCache = null;

            // clear hierarchy cache
            _commandHierarchy = null;

            // clear describer cache
            _commandDescriberCache = null;

            _additionalDescribers.AddRange(cliCommandDescribers ?? Array.Empty<CliCommandDescriber>());
        }

        internal void DisableAnnotatedCommand()
        {
            // clear info cache
            _commandInfoCache = null;

            // clear hierarchy cache
            _commandHierarchy = null;

            // use describer cache from additional describer => won't use annotated describers
            _commandDescriberCache = _additionalDescribers;
        }
    }
}