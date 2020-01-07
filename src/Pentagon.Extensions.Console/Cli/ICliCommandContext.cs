// -----------------------------------------------------------------------
//  <copyright file="ICliCommandContext.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System.Collections.Generic;
    using Collections.Tree;
    using JetBrains.Annotations;

    public interface ICliCommandContext
    {
        [NotNull]
        CliRootCommandInfo RootCommandInfo { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyList<CliCommandInfo> CommandInfos { get; }

        [NotNull]
        [ItemNotNull]
        HierarchyList<CliCommandDescriber> CommandHierarchy { get; }

        [NotNull]
        CliCommandDescriber RootCommandDescriber { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyList<CliCommandDescriber> CommandDescribers { get; }
    }
}