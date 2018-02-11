// -----------------------------------------------------------------------
//  <copyright file="OS.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console
{
    using System.Runtime.InteropServices;

    public static class OS
    {
        static OSPlatform _osPlatform;
        static OperatingSystemPlatform _platform;

        public static OSPlatform OSPlatform
        {
            get
            {
                if (_osPlatform == default(OSPlatform))
                    Initialize();
                return _osPlatform;
            }
        }

        public static OperatingSystemPlatform Platform
        {
            get
            {
                if (_platform == OperatingSystemPlatform.Unspecified)
                    Initialize();
                return _platform;
            }
        }

        static void Initialize()
        {
            _osPlatform = OSPlatform.Windows;
            _platform = OperatingSystemPlatform.Windows;
            if (RuntimeInformation.IsOSPlatform(OSPlatform))
                return;

            _osPlatform = OSPlatform.Linux;
            _platform = OperatingSystemPlatform.Linux;
            if (RuntimeInformation.IsOSPlatform(OSPlatform))
                return;

            _osPlatform = OSPlatform.OSX;
            _platform = OperatingSystemPlatform.OSX;
        }
    }
}