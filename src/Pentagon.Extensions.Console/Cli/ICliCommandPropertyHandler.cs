// -----------------------------------------------------------------------
//  <copyright file="ICliCommandPropertyHandler.cs">
//   Copyright (c) Michal Pokorn�. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using JetBrains.Annotations;

    public interface ICliCommandPropertyHandler<TCommand>
    {
        [NotNull]
        TCommand Command { get; }
    }
}