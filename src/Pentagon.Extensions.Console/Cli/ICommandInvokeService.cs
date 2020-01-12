// -----------------------------------------------------------------------
//  <copyright file="ICommandInvokeService.cs">
//   Copyright (c) Michal Pokorn�. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Cli
{
    using System.Threading.Tasks;

    public interface ICommandInvokeService
    {
        Task ProcessAsync(object command);
    }
}