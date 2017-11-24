// -----------------------------------------------------------------------
//  <copyright file="NativeMethods.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Native
{
    using System;
    using System.Runtime.InteropServices;
    using Flags;

    /// <summary> Represent a methods from Windows API. </summary>
    static class NativeMethods
    {
        public const string UserDll = "user32";

        public const int GWL_STYLE = -16;

        [DllImport(UserDll)]
        public static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

        [DllImport(UserDll)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport(UserDll)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport(UserDll)]
        public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        [DllImport(UserDll)]
        public static extern WindowStyle GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, WindowStyle dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, (int) dwNewLong);
            return new IntPtr(SetWindowLong32(hWnd, nIndex, (int) dwNewLong));
        }

        // new

        [DllImport(UserDll, EntryPoint = "SetWindowLong")]
        static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport(UserDll, EntryPoint = "SetWindowLongPtr")]
        static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}