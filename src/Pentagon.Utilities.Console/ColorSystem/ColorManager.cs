// -----------------------------------------------------------------------
//  <copyright file="ColorManager.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.ColorSystem
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Design.Coloring;
    using Design.Coloring.ColorModels;
    using JetBrains.Annotations;
    using Registration;

    [Register(RegisterType.Transient, typeof(IColorManager))]
    public class ColorManager : IColorManager
    {
        [NotNull]
        readonly IColorMapper _colorMapper;

        [NotNull]
        readonly IColorSchemeManager _schemeManager;

        public ColorManager([NotNull] IColorMapper colorMapper, [NotNull] IColorSchemeManager schemeManager)
        {
            _colorMapper = colorMapper;
            _schemeManager = schemeManager;

            ColorMapping = GetDefaultColors();
        }

        /// <inheritdoc />
        [NotNull]
        public IDictionary<ConsoleColor, Colour> ColorMapping { get; }

        /// <inheritdoc />
        public Colour GetConsoleColor(ConsoleColor consoleColor) => ColorMapping[consoleColor];

        public void SetTheme<TTheme>()
        {
            var codes = _schemeManager.GetEnumThemeCodes<TTheme>();
            for (var i = 0; i < codes.Count; i++)
            {
                var result = _colorMapper.MapColor((ConsoleColor)i, new Colour(new RgbColorModel(codes[i])));
                if (!result)
                    return; // TODO handle error
            }
        }

        IDictionary<ConsoleColor, Colour> GetDefaultColors()
        {
            var map = new ConcurrentDictionary<ConsoleColor, Colour>();

            map.TryAdd(ConsoleColor.Black, DefaultConsoleColorProvider.Black);
            map.TryAdd(ConsoleColor.DarkBlue, DefaultConsoleColorProvider.DarkBlue);
            map.TryAdd(ConsoleColor.DarkGreen, DefaultConsoleColorProvider.DarkGreen);
            map.TryAdd(ConsoleColor.DarkCyan, DefaultConsoleColorProvider.DarkCyan);
            map.TryAdd(ConsoleColor.DarkYellow, DefaultConsoleColorProvider.DarkYellow);
            map.TryAdd(ConsoleColor.DarkGray, DefaultConsoleColorProvider.DarkGray);
            map.TryAdd(ConsoleColor.DarkMagenta, DefaultConsoleColorProvider.DarkMagenta);
            map.TryAdd(ConsoleColor.Blue, DefaultConsoleColorProvider.Blue);
            map.TryAdd(ConsoleColor.Green, DefaultConsoleColorProvider.Green);
            map.TryAdd(ConsoleColor.Cyan, DefaultConsoleColorProvider.Cyan);
            map.TryAdd(ConsoleColor.Yellow, DefaultConsoleColorProvider.Yellow);
            map.TryAdd(ConsoleColor.Gray, DefaultConsoleColorProvider.Gray);
            map.TryAdd(ConsoleColor.Magenta, DefaultConsoleColorProvider.Magenta);
            map.TryAdd(ConsoleColor.White, DefaultConsoleColorProvider.White);
            map.TryAdd(ConsoleColor.Red, DefaultConsoleColorProvider.Red);
            map.TryAdd(ConsoleColor.DarkRed, DefaultConsoleColorProvider.DarkRed);

            return map;
        }
    }
}