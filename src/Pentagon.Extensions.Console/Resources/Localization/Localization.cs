// -----------------------------------------------------------------------
//  <copyright file="Localization.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Resources.Localization
{
    using System.Globalization;
    using System.Reflection;
    using Pentagon.Extensions.Localization.Json;

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