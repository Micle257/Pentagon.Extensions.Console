// -----------------------------------------------------------------------
//  <copyright file="OperatingSystem.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console
{
    using System.Runtime.InteropServices;

    public class OperatingSystem
    {
        public OSPlatform Platform { get; private set; }

        public void Initialize()
        {
            Platform = OSPlatform.Windows;
            if (RuntimeInformation.IsOSPlatform(Platform))
                return;

            Platform = OSPlatform.Linux;
            if (RuntimeInformation.IsOSPlatform(Platform))
                return;

            Platform = OSPlatform.OSX;
        }
    }
}