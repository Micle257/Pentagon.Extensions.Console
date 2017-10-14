// -----------------------------------------------------------------------
//  <copyright file="Extensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------
namespace Pentagon.Utilities.Console
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using ColorSystem;

    public static class Extensions
    {
        public static ConsoleColor ToConsoleColor(this object value)
        {
            if (!value.GetType().IsEnum)
                throw new ArgumentException(message: "The type must be an enum.");

            var values = Enum.GetNames(value.GetType());

            if (values.Length != 16)
                throw new ArgumentException(message: "The enum must have 16 items.");

            var index = values.ToList().IndexOf(value.ToString());

            if (value.GetType() == typeof(DefaultColorScheme))
                return (ConsoleColor)Enum.Parse(typeof(ConsoleColor), value.ToString());
            return (ConsoleColor)index;
        }
    }
}