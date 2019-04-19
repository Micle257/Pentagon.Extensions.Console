// -----------------------------------------------------------------------
//  <copyright file="Localization.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Controls
{
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Pentagon.Extensions.Localization.EntityFramework;

    public static class Localization
    {
        static bool _isLoaded;

        public static string Get(string key)
        {
            if (!_isLoaded)
            {
               var s = JsonLocalization.LoadJsonFromAssembly(Assembly.GetExecutingAssembly());
                _isLoaded = true;
            }

            return JsonLocalization.GetCachedResource(CultureInfo.CurrentUICulture, key, true);
        }
    }
}