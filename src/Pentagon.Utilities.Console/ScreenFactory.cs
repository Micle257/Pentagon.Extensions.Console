// -----------------------------------------------------------------------
//  <copyright file="ScreenFactory.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console
{
    using Attributes;
    using Buffers;
    using ColorSystem;

    [Register(RegisterType.Transient, typeof(IScreenProvider))]
    public class ScreenFactory : IScreenProvider
    {
        readonly IConsoleWriter _writer;
        readonly IConsoleCleaner _cleaner;
        readonly IColorManager _colorManager;
        readonly IScreenCellCache _cellCache;

        public ScreenFactory(IConsoleWriter writer, IConsoleCleaner cleaner, IColorManager colorManager, IScreenCellCache cellCache)
        {
            _writer = writer;
            _cleaner = cleaner;
            _colorManager = colorManager;
            _cellCache = cellCache;
        }

        public IScreen Create<T>() => new Screen<T>(_writer, _cleaner, _colorManager, _cellCache);
    }
}