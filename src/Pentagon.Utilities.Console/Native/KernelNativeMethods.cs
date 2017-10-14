// -----------------------------------------------------------------------
//  <copyright file="KernelNativeMethods.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Native
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using Flags;
    using JetBrains.Annotations;
    using Microsoft.Win32.SafeHandles;
    using Structures;

    public static class KernelNativeMethods
    {
        public const string KernelDll = "kernel32";

        //[DllImport(KernelDll)]
        //public static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleMode dwMode);

        //[DllImport(KernelDll)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public static extern bool SetProcessWorkingSetSize(IntPtr process,
        //                                                   UIntPtr minimumWorkingSetSize,
        //                                                   UIntPtr maximumWorkingSetSize);

        //[DllImport(KernelDll)]
        //public static extern uint GetNumberOfConsoleFonts();

        //[DllImport(KernelDll)]
        //public static extern bool GetConsoleFontInfo(IntPtr hOutput,
        //                                             [MarshalAs(UnmanagedType.Bool)] bool bMaximize,
        //                                             uint count,
        //                                             [MarshalAs(UnmanagedType.LPArray)] [Out] ConsoleFont[] fonts);

        ///// <summary> Retrieves extended information about the current console font. </summary>
        ///// <param name="hConsoleOutput"> A handle to the console screen buffer. </param>
        ///// <param name="bMaximumWindow"> If this parameter is <c> true </c>, font information is retrieved for the maximum window size. If this parameter is <c> false </c>, font information is retrieved for the current window size. </param>
        ///// <param name="lpConsoleCurrentFont"> A pointer to a CONSOLE_FONT_INFOEX structure that receives the requested font information. </param>
        //[DllImport(KernelDll, CharSet = CharSet.Unicode, SetLastError = true)]
        //public static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow, [In] [Out] ConsoleFontEx lpConsoleCurrentFont);

        //[DllImport(KernelDll)]
        //public static extern bool SetConsoleIcon(IntPtr hIcon);

        //[DllImport(KernelDll)]
        //public static extern bool SetConsoleFont(IntPtr hOutput, uint index);

        //[DllImport(KernelDll, SetLastError = true)]
        //public static extern bool SetCurrentConsoleFontEx(
        //    IntPtr ConsoleOutput,
        //    bool MaximumWindow,
        //    ConsoleFontEx ConsoleCurrentFontEx);

        //public static ConsoleMode GetConsoleMode(IntPtr hConsoleHandle)
        //{
        //    uint lmode = 0;
        //    GetConsoleMode(hConsoleHandle, out lmode);
        //    return (ConsoleMode)lmode;
        //}

        //public static uint GetCreationFlags(ProcessCreationFlags creationFlags) => (uint)creationFlags;

        //[DllImport(KernelDll)]
        //public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        //new

        [DllImport(KernelDll)]
        public static extern bool AllocConsole();

        [DllImport(KernelDll, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            ref SecurityAttributes lpProcessAttributes,
            ref SecurityAttributes lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref StartupInfo lpStartupInfo,
            out ProcessInformation lpProcessInformation);

        [NotNull]
        public static Win32Exception GetLastError() => new Win32Exception(Marshal.GetLastWin32Error());

        public static IntPtr GetStdHandle(StdHandle nStdHandle)
        {
            var handle = GetStdHandle((int) nStdHandle);
            if (handle == null || handle == Constants.InvalidHandleValue)
                throw GetLastError();
            return handle;
        }

        public static void SetStdHandle(StdHandle stdHandle, IntPtr handle)
        {
            if (handle == null || handle == Constants.InvalidHandleValue)
                throw new ArgumentException(message: "The handle is not valid.");
            var result = SetStdHandle((int) stdHandle, handle);
            if (!result)
                throw GetLastError();
        }

        public static CONSOLE_SCREEN_BUFFER_INFOEX GetConsoleScreenBufferInfoEx(IntPtr consoleOutput)
        {
            var csbe = new CONSOLE_SCREEN_BUFFER_INFOEX();
            csbe.cbSize = (uint) Marshal.SizeOf(typeof(CONSOLE_SCREEN_BUFFER_INFOEX));
            if (consoleOutput == Constants.InvalidHandleValue)
                return null;
            var result = GetConsoleScreenBufferInfoEx(consoleOutput, ref csbe);
            if (!result)
                throw GetLastError();
            return csbe;
        }

        public static void SetConsoleScreenBufferInfo(IntPtr consoleOutput, ref CONSOLE_SCREEN_BUFFER_INFOEX csbe)
        {
            var result = SetConsoleScreenBufferInfoEx(consoleOutput, ref csbe);
            if (!result)
                throw GetLastError();
        }

        public static void SetOutputToConsole()
        {
            if (!AllocConsole())
                throw GetLastError();

            var handle = GetStdHandle(StdHandle.OutputHandle);
            var fileStream = new FileStream(new SafeFileHandle(handle, true), FileAccess.Write);
            var encoding = Encoding.GetEncoding(437);
            var output = new StreamWriter(fileStream, encoding);
            output.AutoFlush = true;
            Console.SetOut(output);
        }

        public static ConsoleFontInfoEx GetCurrentConsoleFont(IntPtr consoleOutput, bool isMaximumWindow)
        {
            var cfi = new ConsoleFontInfoEx();
            var result = GetCurrentConsoleFontEx(consoleOutput, isMaximumWindow, cfi);
            if (!result)
                throw GetLastError();
            return cfi;
        }

        public static IntPtr GetConsoleWindowHandle()
        {
            var hwnd = GetConsoleWindow();
            if (hwnd == IntPtr.Zero || hwnd == Constants.InvalidHandleValue)
                throw GetLastError();
            return hwnd;
        }

        /// <summary> Retrieves a handle to the specified standard device (standard input, standard output, or standard error). </summary>
        /// <param name="nStdHandle"> The standard device. </param>
        /// <returns> A <see cref="IntPtr" /> handle to the specified device, or a redirected handle. </returns>
        [DllImport(KernelDll, SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport(KernelDll, SetLastError = true)]
        static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);

        [DllImport(KernelDll, SetLastError = true)]
        static extern bool GetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFOEX csbe);

        [DllImport(KernelDll, SetLastError = true)]
        static extern bool SetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFOEX csbe);

        [DllImport(KernelDll, SetLastError = true)]
        static extern bool GetCurrentConsoleFontEx(
            IntPtr consoleOutput,
            bool maximumWindow,
            [In] [Out] ConsoleFontInfoEx lpConsoleCurrentFontEx);

        /// <summary>
        ///     Retrieves the window handle used by the console associated with the calling process. <seealso
        ///                                                                                              cref="https://docs.microsoft.com/en-us/windows/console/getconsolewindow" />, <seealso
        ///                                                                                                                                                                               cref="http://www.pinvoke.net/default.aspx/kernel32/GetConsoleWindow.html" />
        /// </summary>
        /// <returns> A <see cref="IntPtr" /> handle to the window used by the console associated with the calling process. </returns>
        [DllImport(KernelDll)]
        static extern IntPtr GetConsoleWindow();
    }
}