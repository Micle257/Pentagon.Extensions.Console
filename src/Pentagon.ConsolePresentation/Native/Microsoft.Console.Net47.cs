// -----------------------------------------------------------------------
//  <copyright file="Microsoft.Console.Net47.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace System
{
    using Diagnostics.Contracts;
    using IO;
    using Microsoft.Win32.SafeHandles;
    using Runtime.CompilerServices;
    using Runtime.ConstrainedExecution;
    using Runtime.InteropServices;
    using Runtime.Versioning;
    using Security;
    using Security.Permissions;
    using Text;
    using Threading;

    // Provides static fields for console input & output.  Use 
    // Console.In for input from the standard input stream (stdin),
    // Console.Out for output to stdout, and Console.Error
    // for output to stderr.  If any of those console streams are 
    // redirected from the command line, these streams will be redirected.
    // A program can also redirect its own output or input with the 
    // SetIn, SetOut, and SetError methods.
    // 
    // The distinction between Console.Out & Console.Error is useful
    // for programs that redirect output to a file or a pipe.  Note that
    // stdout & stderr can be output to different files at the same
    // time from the DOS command line:
    // 
    // someProgram 1> out 2> err
    // 
    //Contains only static data.  Serializable attribute not required.
    public static class Console
    {
        const int DefaultConsoleBufferSize = 256;
        const short AltVKCode = 0x12;

#if !FEATURE_PAL
        const int NumberLockVKCode = 0x90; // virtual key code
        const int CapsLockVKCode = 0x14;

        // Beep range - see MSDN.
        const int MinBeepFrequency = 37;

        const int MaxBeepFrequency = 32767;

        // MSDN says console titles can be up to 64 KB in length.
        // But I get an exception if I use buffer lengths longer than
        // ~24500 Unicode characters.  Oh well.
        const int MaxConsoleTitleLength = 24500;
#endif // !FEATURE_PAL

#if !FEATURE_CORECLR
        static readonly UnicodeEncoding StdConUnicodeEncoding = new UnicodeEncoding(false, false);
#endif // !FEATURE_CORECLR

        static volatile TextReader _in;
        static volatile TextWriter _out;
        static volatile TextWriter _error;

        static volatile ConsoleCancelEventHandler _cancelCallbacks;
        static volatile ControlCHooker _hooker;

#if !FEATURE_PAL
        // ReadLine & Read can't use this because they need to use ReadFile
        // to be able to handle redirected input.  We have to accept that
        // we will lose repeated keystrokes when someone switches from
        // calling ReadKey to calling Read or ReadLine.  Those methods should 
        // ideally flush this cache as well.
        [SecurityCritical] // auto-generated
        static Win32Native.InputRecord _cachedInputRecord;

        // For ResetColor
        static volatile bool _haveReadDefaultColors;

        static volatile byte _defaultColors;
#endif // !FEATURE_PAL
#if FEATURE_CODEPAGES_FILE // if no codepages file then locked into default       
        private static volatile bool _isOutTextWriterRedirected = false;
        private static volatile bool _isErrorTextWriterRedirected = false;
#endif
        static volatile Encoding _inputEncoding;
        static volatile Encoding _outputEncoding;

#if !FEATURE_CORECLR
        static volatile bool _stdInRedirectQueried;
        static volatile bool _stdOutRedirectQueried;
        static volatile bool _stdErrRedirectQueried;

        static bool _isStdInRedirected;
        static bool _isStdOutRedirected;
        static bool _isStdErrRedirected;
#endif // !FEATURE_CORECLR

        // Private object for locking instead of locking on a public type for SQL reliability work.
        // Use this for internal synchronization during initialization, wiring up events, or for short, non-blocking OS calls.
        static volatile object s_InternalSyncObject;

        static object InternalSyncObject
        {
            get
            {
                Contract.Ensures(Contract.Result<object>() != null);
                if (s_InternalSyncObject == null)
                {
                    var o = new object();
#pragma warning disable 0420
                    Interlocked.CompareExchange<object>(ref s_InternalSyncObject, o, null);
#pragma warning restore 0420
                }
                return s_InternalSyncObject;
            }
        }

        // Use this for blocking in Console.ReadKey, which needs to protect itself in case multiple threads call it simultaneously.
        // Use a ReadKey-specific lock though, to allow other fields to be initialized on this type.
        static volatile object s_ReadKeySyncObject;

        static object ReadKeySyncObject
        {
            get
            {
                Contract.Ensures(Contract.Result<object>() != null);
                if (s_ReadKeySyncObject == null)
                {
                    var o = new object();
#pragma warning disable 0420
                    Interlocked.CompareExchange<object>(ref s_ReadKeySyncObject, o, null);
#pragma warning restore 0420
                }
                return s_ReadKeySyncObject;
            }
        }

        // About reliability: I'm not using SafeHandle here.  We don't 
        // need to close these handles, and we don't allow the user to close
        // them so we don't have many of the security problems inherent in
        // something like file handles.  Additionally, in a host like SQL 
        // Server, we won't have a console.
        static volatile IntPtr _consoleInputHandle;

        static volatile IntPtr _consoleOutputHandle;

        static IntPtr ConsoleInputHandle
        {
            [SecurityCritical] // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            get
            {
                if (_consoleInputHandle == IntPtr.Zero)
                    _consoleInputHandle = Win32Native.GetStdHandle(Win32Native.STD_INPUT_HANDLE);
                return _consoleInputHandle;
            }
        }

        static IntPtr ConsoleOutputHandle
        {
            [SecurityCritical] // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            get
            {
                if (_consoleOutputHandle == IntPtr.Zero)
                    _consoleOutputHandle = Win32Native.GetStdHandle(Win32Native.STD_OUTPUT_HANDLE);
                return _consoleOutputHandle;
            }
        }

#if !FEATURE_CORECLR
        [SecuritySafeCritical]
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        static bool IsHandleRedirected(IntPtr ioHandle)
        {
            // Need this to use GetFileType:
            var safeIOHandle = new SafeFileHandle(ioHandle, false);

            // If handle is not to a character device, we must be redirected:
            int fileType = Win32Native.GetFileType(safeIOHandle);
            if ((fileType & Win32Native.FILE_TYPE_CHAR) != Win32Native.FILE_TYPE_CHAR)
                return true;

            // We are on a char device.
            // If GetConsoleMode succeeds, we are NOT redirected.
            int mode;
            bool success = Win32Native.GetConsoleMode(ioHandle, out mode);
            return !success;
        }

        public static bool IsInputRedirected
        {
            [SecuritySafeCritical]
            get
            {
                if (_stdInRedirectQueried)
                    return _isStdInRedirected;

                lock (InternalSyncObject)
                {
                    if (_stdInRedirectQueried)
                        return _isStdInRedirected;

                    _isStdInRedirected = IsHandleRedirected(ConsoleInputHandle);
                    _stdInRedirectQueried = true;

                    return _isStdInRedirected;
                }
            }
        } // public static bool IsInputRedirected

        public static bool IsOutputRedirected
        {
            [SecuritySafeCritical]
            get
            {
                if (_stdOutRedirectQueried)
                    return _isStdOutRedirected;

                lock (InternalSyncObject)
                {
                    if (_stdOutRedirectQueried)
                        return _isStdOutRedirected;

                    _isStdOutRedirected = IsHandleRedirected(ConsoleOutputHandle);
                    _stdOutRedirectQueried = true;

                    return _isStdOutRedirected;
                }
            }
        } // public static bool IsOutputRedirected

        public static bool IsErrorRedirected
        {
            [SecuritySafeCritical]
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            get
            {
                if (_stdErrRedirectQueried)
                    return _isStdErrRedirected;

                lock (InternalSyncObject)
                {
                    if (_stdErrRedirectQueried)
                        return _isStdErrRedirected;

                    IntPtr errHndle = Win32Native.GetStdHandle(Win32Native.STD_ERROR_HANDLE);
                    _isStdErrRedirected = IsHandleRedirected(errHndle);
                    _stdErrRedirectQueried = true;

                    return _isStdErrRedirected;
                }
            }
        } // public static bool IsErrorRedirected
#endif // !FEATURE_CORECLR

        public static TextReader In
        {
            [SecuritySafeCritical] // auto-generated
            [HostProtection(UI = true)]
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                Contract.Ensures(Contract.Result<TextReader>() != null);
                // Because most applications don't use stdin, we can delay 
                // initialize it slightly better startup performance.
                if (_in == null)
                {
                    lock (InternalSyncObject)
                    {
                        if (_in == null)
                        {
                            // Set up Console.In
                            var s = OpenStandardInput(DefaultConsoleBufferSize);
                            TextReader tr;
                            if (s == Stream.Null)
                                tr = StreamReader.Null;
                            else
                            {
                                // Hopefully Encoding.GetEncoding doesn't load as many classes now.
#if FEATURE_CORECLR
                                Encoding enc = Encoding.UTF8;
#else // FEATURE_CORECLR                              
                                var enc = InputEncoding;
#endif // FEATURE_CORECLR
                                tr = TextReader.Synchronized(new StreamReader(s, enc, false, DefaultConsoleBufferSize, true));
                            }
                            Thread.MemoryBarrier();
                            _in = tr;
                        }
                    }
                }
                return _in;
            }
        }

        public static TextWriter Out
        {
            [HostProtection(UI = true)]
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                Contract.Ensures(Contract.Result<TextWriter>() != null);
                // Hopefully this is inlineable.
                if (_out == null)
                    InitializeStdOutError(true);
                return _out;
            }
        }

        public static TextWriter Error
        {
            [HostProtection(UI = true)]
            get
            {
                Contract.Ensures(Contract.Result<TextWriter>() != null);
                // Hopefully this is inlineable.
                if (_error == null)
                    InitializeStdOutError(false);
                return _error;
            }
        }

        // For console apps, the console handles are set to values like 3, 7, 
        // and 11 OR if you've been created via CreateProcess, possibly -1
        // or 0.  -1 is definitely invalid, while 0 is probably invalid.
        // Also note each handle can independently be invalid or good.
        // For Windows apps, the console handles are set to values like 3, 7, 
        // and 11 but are invalid handles - you may not write to them.  However,
        // you can still spawn a Windows app via CreateProcess and read stdout
        // and stderr.
        // So, we always need to check each handle independently for validity
        // by trying to write or read to it, unless it is -1.

        // We do not do a security check here, under the assumption that this
        // cannot create a security hole, but only waste a user's time or 
        // cause a possible denial of service attack.
        [SecuritySafeCritical] // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        static void InitializeStdOutError(bool stdout)
        {
            // Set up Console.Out or Console.Error.
            lock (InternalSyncObject)
            {
                if (stdout && _out != null)
                    return;
                if (!stdout && _error != null)
                    return;

                TextWriter writer = null;
                Stream s;
                if (stdout)
                    s = OpenStandardOutput(DefaultConsoleBufferSize);
                else
                    s = OpenStandardError(DefaultConsoleBufferSize);

                if (s == Stream.Null)
                {
#if _DEBUG
                    if (CheckOutputDebug())
                        writer = MakeDebugOutputTextWriter((stdout) ? "Console.Out: " : "Console.Error: ");
                    else
#endif // _DEBUG
                    writer = TextWriter.Synchronized(StreamWriter.Null);
                }
                else
                {
#if FEATURE_CORECLR                    
                    Encoding encoding = Encoding.UTF8;
#else // FEATURE_CORECLR                    
                    var encoding = OutputEncoding;
#endif // FEATURE_CORECLR
                    var stdxxx = new StreamWriter(s, encoding, DefaultConsoleBufferSize, true);
                    stdxxx.HaveWrittenPreamble = true;
                    stdxxx.AutoFlush = true;
                    writer = TextWriter.Synchronized(stdxxx);
                }
                if (stdout)
                    _out = writer;
                else
                    _error = writer;
                Contract.Assert(stdout && _out != null || !stdout && _error != null, userMessage: "Didn't set Console::_out or _error appropriately!");
            }
        }

        // This is ONLY used in debug builds.  If you have a registry key set,
        // it will redirect Console.Out & Error on console-less applications to
        // your debugger's output window.
#if _DEBUG
        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private static bool CheckOutputDebug()
        {
#if FEATURE_WIN32_REGISTRY

            new System.Security.Permissions.RegistryPermission(RegistryPermissionAccess.Read | RegistryPermissionAccess.Write, "HKEY_LOCAL_MACHINE").Assert();
            RegistryKey rk = Registry.LocalMachine;               
            using (rk = rk.OpenSubKey("Software\\Microsoft\\.NETFramework", false)) {
                if (rk != null) {
                    Object obj = rk.GetValue("ConsoleSpewToDebugger", 0);
                    if (obj != null && ((int)obj) != 0) {
                        return true;
                    }
                }
            }
            return false;
#else // FEATURE_WIN32_REGISTRY    
#if FEATURE_PAL && !FEATURE_CORECLR
            const int parameterValueLength = 255;
            StringBuilder parameterValue = new StringBuilder(parameterValueLength);
            bool rc = Win32Native.FetchConfigurationString(true, "ConsoleSpewToDebugger", parameterValue, parameterValueLength);
            if (rc) {
                if (0 != parameterValue.Length) {
                    int value = Convert.ToInt32(parameterValue.ToString());
                    if (0 != value)
                        return true;
                }
            }
#endif // FEATURE_PAL && !FEATURE_CORECLR
            return false;
#endif // FEATURE_WIN32_REGISTRY       
        }
#endif // _DEBUG

#if _DEBUG
        private static TextWriter MakeDebugOutputTextWriter(String streamLabel)
        {
            TextWriter output = new __DebugOutputTextWriter(streamLabel);
            output.WriteLine("Output redirected to debugger from a bit bucket.");
            return TextWriter.Synchronized(output);
        }
#endif // _DEBUG

#if !FEATURE_CORECLR
        // We cannot simply compare the encoding to Encoding.Unicode bacasue it incorporates BOM
        // and we do not care about BOM. Instead, we compare by class, codepage and little-endianess only:
        static bool IsStandardConsoleUnicodeEncoding(Encoding encoding)
        {
            var enc = encoding as UnicodeEncoding;
            if (null == enc)
                return false;

            return StdConUnicodeEncoding.CodePage == enc.CodePage
                   && StdConUnicodeEncoding.bigEndian == enc.bigEndian;
        }

        static bool GetUseFileAPIs(int handleType)
        {
            switch (handleType)
            {
                case Win32Native.STD_INPUT_HANDLE:
                    return !IsStandardConsoleUnicodeEncoding(InputEncoding) || IsInputRedirected;

                case Win32Native.STD_OUTPUT_HANDLE:
                    return !IsStandardConsoleUnicodeEncoding(OutputEncoding) || IsOutputRedirected;

                case Win32Native.STD_ERROR_HANDLE:
                    return !IsStandardConsoleUnicodeEncoding(OutputEncoding) || IsErrorRedirected;

                default:
                    // This can never happen.
                    Contract.Assert(false, "Unexpected handleType value (" + handleType + ")");
                    return true;
            }
        }
#endif // !FEATURE_CORECLR

        // This method is only exposed via methods to get at the console.
        // We won't use any security checks here.
#if FEATURE_CORECLR
        [System.Security.SecurityCritical] // auto-generated
#else
        [SecuritySafeCritical]
#endif
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        static Stream GetStandardFile(int stdHandleName, FileAccess access, int bufferSize)
        {
            // We shouldn't close the handle for stdout, etc, or we'll break
            // unmanaged code in the process that will print to console.
            // We should have a better way of marking this on SafeHandle.
            IntPtr handle = Win32Native.GetStdHandle(stdHandleName);
            var sh = new SafeFileHandle(handle, false);

            // If someone launches a managed process via CreateProcess, stdout
            // stderr, & stdin could independently be set to INVALID_HANDLE_VALUE.
            // Additionally they might use 0 as an invalid handle.
            if (sh.IsInvalid)
            {
                // Minor perf optimization - get it out of the finalizer queue.
                sh.SetHandleAsInvalid();
                return Stream.Null;
            }

            // Check whether we can read or write to this handle.
            if (stdHandleName != Win32Native.STD_INPUT_HANDLE && !ConsoleHandleIsWritable(sh))
            {
                //BCLDebug.ConsoleError("Console::ConsoleHandleIsValid for std handle "+stdHandleName+" failed, setting it to a null stream");
                return Stream.Null;
            }

#if !FEATURE_CORECLR
            var useFileAPIs = GetUseFileAPIs(stdHandleName);
#else
            const bool useFileAPIs = true;
#endif // !FEATURE_CORECLR

            //BCLDebug.ConsoleError("Console::GetStandardFile for std handle "+stdHandleName+" succeeded, returning handle number "+handle.ToString());
            Stream console = new __ConsoleStream(sh, access, useFileAPIs);
            // Do not buffer console streams, or we can get into situations where
            // we end up blocking waiting for you to hit enter twice.  It was
            // redundant.  
            return console;
        }

        // Checks whether stdout or stderr are writable.  Do NOT pass
        // stdin here.
#if FEATURE_CORECLR
        [System.Security.SecurityCritical] // auto-generated
#else
        [SecuritySafeCritical]
#endif
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        static unsafe bool ConsoleHandleIsWritable(SafeFileHandle outErrHandle)
        {
            // Do NOT call this method on stdin!

            // Windows apps may have non-null valid looking handle values for 
            // stdin, stdout and stderr, but they may not be readable or 
            // writable.  Verify this by calling WriteFile in the 
            // appropriate modes.
            // This must handle console-less Windows apps.

            int bytesWritten;
            byte junkByte = 0x41;
            int r = Win32Native.WriteFile(outErrHandle, &junkByte, 0, out bytesWritten, IntPtr.Zero);
            // In Win32 apps w/ no console, bResult should be 0 for failure.
            return r != 0;
        }

        public static Encoding InputEncoding
        {
            [SecuritySafeCritical] // auto-generated
            get
            {
                Contract.Ensures(Contract.Result<Encoding>() != null);

                if (null != _inputEncoding)
                    return _inputEncoding;

                lock (InternalSyncObject)
                {
                    if (null != _inputEncoding)
                        return _inputEncoding;

                    uint cp = Win32Native.GetConsoleCP();
                    _inputEncoding = Encoding.GetEncoding((int) cp);
                    return _inputEncoding;
                }
            }
#if FEATURE_CODEPAGES_FILE // if no codepages file then locked into default                                                           
            [System.Security.SecuritySafeCritical]  // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            set {
 
                if (value == null)
                    throw new ArgumentNullException("value");
 
                Contract.EndContractBlock();
 
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
 
                lock(InternalSyncObject) {
 
                    if (!IsStandardConsoleUnicodeEncoding(value)) {
 
                        uint cp = (uint) value.CodePage;
                        bool r = Win32Native.SetConsoleCP(cp);
                        if (!r)
                            __Error.WinIOError();
                    }
 
                    _inputEncoding = (Encoding) value.Clone();
 
                    // We need to reinitialize Console.In in the next call to _in
                    // This will discard the current StreamReader, potentially 
                    // losing buffered data
                    _in = null;
                }
            }  // set
#endif // FEATURE_CODEPAGES_FILE
        } // public static Encoding InputEncoding

        public static Encoding OutputEncoding
        {
            [SecuritySafeCritical] // auto-generated
            get
            {
                Contract.Ensures(Contract.Result<Encoding>() != null);

                if (null != _outputEncoding)
                    return _outputEncoding;

                lock (InternalSyncObject)
                {
                    if (null != _outputEncoding)
                        return _outputEncoding;

                    uint cp = Win32Native.GetConsoleOutputCP();
                    _outputEncoding = Encoding.GetEncoding((int) cp);
                    return _outputEncoding;
                }
            }
#if FEATURE_CODEPAGES_FILE // if no codepages file then locked into default                                                                       
            [System.Security.SecuritySafeCritical]  // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            set {
                if (value == null)
                    throw new ArgumentNullException("value");
                Contract.EndContractBlock();
 
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();
 
                lock(InternalSyncObject) {
                    // Before changing the code page we need to flush the data 
                    // if Out hasn't been redirected. Also, have the next call to  
                    // _out reinitialize the console code page.
 
                    if (_out != null && !_isOutTextWriterRedirected) {
                        _out.Flush();
                        _out = null;
                    }
                    if (_error != null && !_isErrorTextWriterRedirected) {
                        _error.Flush();
                        _error = null;
                    }
 
                    if (!IsStandardConsoleUnicodeEncoding(value)) {
 
                        uint cp = (uint) value.CodePage;
                        bool r = Win32Native.SetConsoleOutputCP(cp);
                        if (!r)
                            __Error.WinIOError();
                    }
 
                    _outputEncoding = (Encoding) value.Clone();
                }
            }  // set
#endif // FEATURE_CODEPAGES_FILE    
        } // public static Encoding OutputEncoding

#if !FEATURE_PAL
        [HostProtection(UI = true)]
        public static void Beep()
        {
            Beep(800, 200);
        }

        [SecuritySafeCritical] // auto-generated
        [HostProtection(UI = true)]
        public static void Beep(int frequency, int duration)
        {
            if (frequency < MinBeepFrequency || frequency > MaxBeepFrequency)
            {
                throw new ArgumentOutOfRangeException(paramName: "frequency",
                                                      actualValue: frequency,
                                                      message: Environment.GetResourceString("ArgumentOutOfRange_BeepFrequency", MinBeepFrequency, MaxBeepFrequency));
            }
            if (duration <= 0)
                throw new ArgumentOutOfRangeException(paramName: "duration", actualValue: duration, message: Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));

            // Note that Beep over Remote Desktop connections does not currently
            Contract.EndContractBlock();
            // work.  Ignore any failures here.
            Win32Native.Beep(frequency, duration);
        }

        [SecuritySafeCritical] // auto-generated
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static void Clear()
        {
            Win32Native.COORD coordScreen = new Win32Native.COORD();
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi;
            bool success;
            int conSize;

            var hConsole = ConsoleOutputHandle;
            if (hConsole == Win32Native.INVALID_HANDLE_VALUE)
                throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));

            // get the number of character cells in the current buffer
            // Go through my helper method for fetching a screen buffer info
            // to correctly handle default console colors.
            csbi = GetBufferInfo();
            conSize = csbi.dwSize.X * csbi.dwSize.Y;

            // fill the entire screen with blanks

            var numCellsWritten = 0;
            success = Win32Native.FillConsoleOutputCharacter(hConsole,
                                                             ' ',
                                                             conSize,
                                                             coordScreen,
                                                             out numCellsWritten);
            if (!success)
                __Error.WinIOError();

            // now set the buffer's attributes accordingly

            numCellsWritten = 0;
            success = Win32Native.FillConsoleOutputAttribute(hConsole,
                                                             csbi.wAttributes,
                                                             conSize,
                                                             coordScreen,
                                                             out numCellsWritten);
            if (!success)
                __Error.WinIOError();

            // put the cursor at (0, 0)

            success = Win32Native.SetConsoleCursorPosition(hConsole, coordScreen);
            if (!success)
                __Error.WinIOError();
        }

        [SecurityCritical] // auto-generated
        static Win32Native.Color ConsoleColorToColorAttribute(ConsoleColor color, bool isBackground)
        {
            if (((int) color & ~0xf) != 0)
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidConsoleColor"));
            Contract.EndContractBlock();

            Win32Native.Color c = (Win32Native.Color) color;

            // Make these background colors instead of foreground
            if (isBackground)
                c = (Win32Native.Color) ((int) c << 4);
            return c;
        }

        [SecurityCritical] // auto-generated
        static ConsoleColor ColorAttributeToConsoleColor(Win32Native.Color c)
        {
            // Turn background colors into foreground colors.
            if ((c & Win32Native.Color.BackgroundMask) != 0)
                c = (Win32Native.Color) ((int) c >> 4);

            return (ConsoleColor) c;
        }

        public static ConsoleColor BackgroundColor
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                bool succeeded;
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo(false, out succeeded);

                // For code that may be used from Windows app w/ no console
                if (!succeeded)
                    return ConsoleColor.Black;

                Win32Native.Color c = (Win32Native.Color) csbi.wAttributes & Win32Native.Color.BackgroundMask;
                return ColorAttributeToConsoleColor(c);
            }
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            set
            {
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();

                Win32Native.Color c = ConsoleColorToColorAttribute(value, true);

                bool succeeded;
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo(false, out succeeded);
                // For code that may be used from Windows app w/ no console
                if (!succeeded)
                    return;

                Contract.Assert(_haveReadDefaultColors, userMessage: "Setting the foreground color before we've read the default foreground color!");

                short attrs = csbi.wAttributes;
                attrs &= ~(short) Win32Native.Color.BackgroundMask;
                // C#'s bitwise-or sign-extends to 32 bits.
                attrs = (short) ((ushort) attrs | (uint) (ushort) c);
                // Ignore errors here - there are some scenarios for running code that wants
                // to print in colors to the console in a Windows application.
                Win32Native.SetConsoleTextAttribute(ConsoleOutputHandle, attrs);
            }
        } // public static ConsoleColor BackgroundColor

        public static ConsoleColor ForegroundColor
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                bool succeeded;
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo(false, out succeeded);

                // For code that may be used from Windows app w/ no console
                if (!succeeded)
                    return ConsoleColor.Gray;

                Win32Native.Color c = (Win32Native.Color) csbi.wAttributes & Win32Native.Color.ForegroundMask;
                return ColorAttributeToConsoleColor(c);
            }
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            set
            {
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();

                Win32Native.Color c = ConsoleColorToColorAttribute(value, false);

                bool succeeded;
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo(false, out succeeded);
                // For code that may be used from Windows app w/ no console
                if (!succeeded)
                    return;

                Contract.Assert(_haveReadDefaultColors, userMessage: "Setting the foreground color before we've read the default foreground color!");

                short attrs = csbi.wAttributes;
                attrs &= ~(short) Win32Native.Color.ForegroundMask;
                // C#'s bitwise-or sign-extends to 32 bits.
                attrs = (short) ((ushort) attrs | (uint) (ushort) c);
                // Ignore errors here - there are some scenarios for running code that wants
                // to print in colors to the console in a Windows application.
                Win32Native.SetConsoleTextAttribute(ConsoleOutputHandle, attrs);
            }
        } // public static ConsoleColor ForegroundColor

        [SecuritySafeCritical] // auto-generated
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static void ResetColor()
        {
            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();

            bool succeeded;
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo(false, out succeeded);
            // For code that may be used from Windows app w/ no console
            if (!succeeded)
                return;

            Contract.Assert(_haveReadDefaultColors, userMessage: "Setting the foreground color before we've read the default foreground color!");

            short defaultAttrs = _defaultColors;
            // Ignore errors here - there are some scenarios for running code that wants
            // to print in colors to the console in a Windows application.
            Win32Native.SetConsoleTextAttribute(ConsoleOutputHandle, defaultAttrs);
        }

        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static void MoveBufferArea(int sourceLeft,
                                          int sourceTop,
                                          int sourceWidth,
                                          int sourceHeight,
                                          int targetLeft,
                                          int targetTop)
        {
            MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop, ' ', ConsoleColor.Black, BackgroundColor);
        }

        [SecuritySafeCritical] // auto-generated
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static unsafe void MoveBufferArea(int sourceLeft,
                                                 int sourceTop,
                                                 int sourceWidth,
                                                 int sourceHeight,
                                                 int targetLeft,
                                                 int targetTop,
                                                 char sourceChar,
                                                 ConsoleColor sourceForeColor,
                                                 ConsoleColor sourceBackColor)
        {
            if (sourceForeColor < ConsoleColor.Black || sourceForeColor > ConsoleColor.White)
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidConsoleColor"), paramName: "sourceForeColor");
            if (sourceBackColor < ConsoleColor.Black || sourceBackColor > ConsoleColor.White)
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidConsoleColor"), paramName: "sourceBackColor");
            Contract.EndContractBlock();

            Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
            Win32Native.COORD bufferSize = csbi.dwSize;
            if (sourceLeft < 0 || sourceLeft > bufferSize.X)
                throw new ArgumentOutOfRangeException(paramName: "sourceLeft", actualValue: sourceLeft, message: Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
            if (sourceTop < 0 || sourceTop > bufferSize.Y)
                throw new ArgumentOutOfRangeException(paramName: "sourceTop", actualValue: sourceTop, message: Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
            if (sourceWidth < 0 || sourceWidth > bufferSize.X - sourceLeft)
                throw new ArgumentOutOfRangeException(paramName: "sourceWidth", actualValue: sourceWidth, message: Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
            if (sourceHeight < 0 || sourceTop > bufferSize.Y - sourceHeight)
                throw new ArgumentOutOfRangeException(paramName: "sourceHeight", actualValue: sourceHeight, message: Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));

            // Note: if the target range is partially in and partially out
            // of the buffer, then we let the OS clip it for us.
            if (targetLeft < 0 || targetLeft > bufferSize.X)
                throw new ArgumentOutOfRangeException(paramName: "targetLeft", actualValue: targetLeft, message: Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
            if (targetTop < 0 || targetTop > bufferSize.Y)
                throw new ArgumentOutOfRangeException(paramName: "targetTop", actualValue: targetTop, message: Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));

            // If we're not doing any work, bail out now (Windows will return
            // an error otherwise)
            if (sourceWidth == 0 || sourceHeight == 0)
                return;

            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();

            // Read data from the original location, blank it out, then write
            // it to the new location.  This will handle overlapping source and
            // destination regions correctly.

            // See the "Reading and Writing Blocks of Characters and Attributes" 
            // sample for help

            // Read the old data
            Win32Native.CHAR_INFO[] data = new Win32Native.CHAR_INFO[sourceWidth * sourceHeight];
            bufferSize.X = (short) sourceWidth;
            bufferSize.Y = (short) sourceHeight;
            Win32Native.COORD bufferCoord = new Win32Native.COORD();
            Win32Native.SMALL_RECT readRegion = new Win32Native.SMALL_RECT();
            readRegion.Left = (short) sourceLeft;
            readRegion.Right = (short) (sourceLeft + sourceWidth - 1);
            readRegion.Top = (short) sourceTop;
            readRegion.Bottom = (short) (sourceTop + sourceHeight - 1);

            bool r;
            fixed (Win32Native.CHAR_INFO* pCharInfo = data)
            {
                r = Win32Native.ReadConsoleOutput(ConsoleOutputHandle, pCharInfo, bufferSize, bufferCoord, ref readRegion);
            }
            if (!r)
                __Error.WinIOError();

            // Overwrite old section
            // I don't have a good function to blank out a rectangle.
            Win32Native.COORD writeCoord = new Win32Native.COORD();
            writeCoord.X = (short) sourceLeft;
            Win32Native.Color c = ConsoleColorToColorAttribute(sourceBackColor, true);
            c |= ConsoleColorToColorAttribute(sourceForeColor, false);
            var attr = (short) c;
            int numWritten;
            for (var i = sourceTop; i < sourceTop + sourceHeight; i++)
            {
                writeCoord.Y = (short) i;
                r = Win32Native.FillConsoleOutputCharacter(ConsoleOutputHandle, sourceChar, sourceWidth, writeCoord, out numWritten);
                Contract.Assert(numWritten == sourceWidth, userMessage: "FillConsoleOutputCharacter wrote the wrong number of chars!");
                if (!r)
                    __Error.WinIOError();

                r = Win32Native.FillConsoleOutputAttribute(ConsoleOutputHandle, attr, sourceWidth, writeCoord, out numWritten);
                if (!r)
                    __Error.WinIOError();
            }

            // Write text to new location
            Win32Native.SMALL_RECT writeRegion = new Win32Native.SMALL_RECT();
            writeRegion.Left = (short) targetLeft;
            writeRegion.Right = (short) (targetLeft + sourceWidth);
            writeRegion.Top = (short) targetTop;
            writeRegion.Bottom = (short) (targetTop + sourceHeight);

            fixed (Win32Native.CHAR_INFO* pCharInfo = data)
            {
                r = Win32Native.WriteConsoleOutput(ConsoleOutputHandle, pCharInfo, bufferSize, bufferCoord, ref writeRegion);
            }
        } // MoveBufferArea

        [SecurityCritical] // auto-generated
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        static Win32Native.CONSOLE_SCREEN_BUFFER_INFO GetBufferInfo()
        {
            bool junk;
            return GetBufferInfo(true, out junk);
        }

        // For apps that don't have a console (like Windows apps), they might
        // run other code that includes color console output.  Allow a mechanism
        // where that code won't throw an exception for simple errors.
        [SecuritySafeCritical] // auto-generated
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        static Win32Native.CONSOLE_SCREEN_BUFFER_INFO GetBufferInfo(bool throwOnNoConsole, out bool succeeded)
        {
            succeeded = false;
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi;
            bool success;

            var hConsole = ConsoleOutputHandle;
            if (hConsole == Win32Native.INVALID_HANDLE_VALUE)
            {
                if (!throwOnNoConsole)
                    return new Win32Native.CONSOLE_SCREEN_BUFFER_INFO();
                throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
            }

            // Note that if stdout is redirected to a file, the console handle
            // may be a file.  If this fails, try stderr and stdin.
            success = Win32Native.GetConsoleScreenBufferInfo(hConsole, out csbi);
            if (!success)
            {
                success = Win32Native.GetConsoleScreenBufferInfo(Win32Native.GetStdHandle(Win32Native.STD_ERROR_HANDLE), out csbi);
                if (!success)
                    success = Win32Native.GetConsoleScreenBufferInfo(Win32Native.GetStdHandle(Win32Native.STD_INPUT_HANDLE), out csbi);

                if (!success)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    if (errorCode == Win32Native.ERROR_INVALID_HANDLE && !throwOnNoConsole)
                        return new Win32Native.CONSOLE_SCREEN_BUFFER_INFO();
                    __Error.WinIOError(errorCode, null);
                }
            }

            if (!_haveReadDefaultColors)
            {
                // Fetch the default foreground and background color for the
                // ResetColor method.
                Contract.Assert((int) Win32Native.Color.ColorMask == 0xff, userMessage: "Make sure one byte is large enough to store a Console color value!");
                _defaultColors = (byte) (csbi.wAttributes & (short) Win32Native.Color.ColorMask);
                _haveReadDefaultColors = true;
            }

            succeeded = true;
            return csbi;
        }

        public static int BufferHeight
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            get
            {
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
                return csbi.dwSize.Y;
            }
            [ResourceExposure(ResourceScope.Process)] [ResourceConsumption(ResourceScope.Process)] set { SetBufferSize(BufferWidth, value); }
        }

        public static int BufferWidth
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            get
            {
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
                return csbi.dwSize.X;
            }
            [ResourceExposure(ResourceScope.Process)] [ResourceConsumption(ResourceScope.Process)] set { SetBufferSize(value, BufferHeight); }
        }

        [SecuritySafeCritical] // auto-generated
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static void SetBufferSize(int width, int height)
        {
            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();

            // Ensure the new size is not smaller than the console window
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
            Win32Native.SMALL_RECT srWindow = csbi.srWindow;
            if (width < srWindow.Right + 1 || width >= short.MaxValue)
                throw new ArgumentOutOfRangeException(paramName: "width", actualValue: width, message: Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferLessThanWindowSize"));
            if (height < srWindow.Bottom + 1 || height >= short.MaxValue)
                throw new ArgumentOutOfRangeException(paramName: "height", actualValue: height, message: Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferLessThanWindowSize"));

            Win32Native.COORD size = new Win32Native.COORD();
            size.X = (short) width;
            size.Y = (short) height;
            bool r = Win32Native.SetConsoleScreenBufferSize(ConsoleOutputHandle, size);
            if (!r)
                __Error.WinIOError();
        }

        public static int WindowHeight
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
                return csbi.srWindow.Bottom - csbi.srWindow.Top + 1;
            }
            [ResourceExposure(ResourceScope.Process)] [ResourceConsumption(ResourceScope.Process)] set { SetWindowSize(WindowWidth, value); }
        }

        public static int WindowWidth
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
                return csbi.srWindow.Right - csbi.srWindow.Left + 1;
            }
            [ResourceExposure(ResourceScope.Process)] [ResourceConsumption(ResourceScope.Process)] set { SetWindowSize(value, WindowHeight); }
        }

        [SecuritySafeCritical] // auto-generated
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static unsafe void SetWindowSize(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(paramName: "width", actualValue: width, message: Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(paramName: "height", actualValue: height, message: Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            Contract.EndContractBlock();

            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();

            // Get the position of the current console window
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
            bool r;

            // If the buffer is smaller than this new window size, resize the
            // buffer to be large enough.  Include window position.
            var resizeBuffer = false;
            Win32Native.COORD size = new Win32Native.COORD();
            size.X = csbi.dwSize.X;
            size.Y = csbi.dwSize.Y;
            if (csbi.dwSize.X < csbi.srWindow.Left + width)
            {
                if (csbi.srWindow.Left >= short.MaxValue - width)
                    throw new ArgumentOutOfRangeException("width", Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowBufferSize"));
                size.X = (short) (csbi.srWindow.Left + width);
                resizeBuffer = true;
            }
            if (csbi.dwSize.Y < csbi.srWindow.Top + height)
            {
                if (csbi.srWindow.Top >= short.MaxValue - height)
                    throw new ArgumentOutOfRangeException("height", Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowBufferSize"));
                size.Y = (short) (csbi.srWindow.Top + height);
                resizeBuffer = true;
            }
            if (resizeBuffer)
            {
                r = Win32Native.SetConsoleScreenBufferSize(ConsoleOutputHandle, size);
                if (!r)
                    __Error.WinIOError();
            }

            Win32Native.SMALL_RECT srWindow = csbi.srWindow;
            // Preserve the position, but change the size.
            srWindow.Bottom = (short) (srWindow.Top + height - 1);
            srWindow.Right = (short) (srWindow.Left + width - 1);

            r = Win32Native.SetConsoleWindowInfo(ConsoleOutputHandle, true, &srWindow);
            if (!r)
            {
                var errorCode = Marshal.GetLastWin32Error();

                // If we resized the buffer, un-resize it.
                if (resizeBuffer)
                    Win32Native.SetConsoleScreenBufferSize(ConsoleOutputHandle, csbi.dwSize);

                // Try to give a better error message here
                Win32Native.COORD bounds = Win32Native.GetLargestConsoleWindowSize(ConsoleOutputHandle);
                if (width > bounds.X)
                    throw new ArgumentOutOfRangeException(paramName: "width", actualValue: width, message: Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowSize_Size", bounds.X));
                if (height > bounds.Y)
                    throw new ArgumentOutOfRangeException(paramName: "height", actualValue: height, message: Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowSize_Size", bounds.Y));

                __Error.WinIOError(errorCode, string.Empty);
            }
        } // public static unsafe void SetWindowSize(int width, int height)

        public static int LargestWindowWidth
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                // Note this varies based on current screen resolution and 
                // current console font.  Do not cache this value.
                Win32Native.COORD bounds = Win32Native.GetLargestConsoleWindowSize(ConsoleOutputHandle);
                return bounds.X;
            }
        }

        public static int LargestWindowHeight
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                // Note this varies based on current screen resolution and 
                // current console font.  Do not cache this value.
                Win32Native.COORD bounds = Win32Native.GetLargestConsoleWindowSize(ConsoleOutputHandle);
                return bounds.Y;
            }
        }

        public static int WindowLeft
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
                return csbi.srWindow.Left;
            }
            [ResourceExposure(ResourceScope.Process)] [ResourceConsumption(ResourceScope.Process)] set { SetWindowPosition(value, WindowTop); }
        }

        public static int WindowTop
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
                return csbi.srWindow.Top;
            }
            [ResourceExposure(ResourceScope.Process)] [ResourceConsumption(ResourceScope.Process)] set { SetWindowPosition(WindowLeft, value); }
        }

        [SecuritySafeCritical] // auto-generated
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static unsafe void SetWindowPosition(int left, int top)
        {
            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();

            // Get the size of the current console window
            Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();

            Win32Native.SMALL_RECT srWindow = csbi.srWindow;

            // Check for arithmetic underflows & overflows.
            var newRight = left + srWindow.Right - srWindow.Left + 1;
            if (left < 0 || newRight > csbi.dwSize.X || newRight < 0)
                throw new ArgumentOutOfRangeException(paramName: "left", actualValue: left, message: Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowPos"));
            var newBottom = top + srWindow.Bottom - srWindow.Top + 1;
            if (top < 0 || newBottom > csbi.dwSize.Y || newBottom < 0)
                throw new ArgumentOutOfRangeException(paramName: "top", actualValue: top, message: Environment.GetResourceString("ArgumentOutOfRange_ConsoleWindowPos"));

            // Preserve the size, but move the position.
            srWindow.Bottom -= (short) (srWindow.Top - top);
            srWindow.Right -= (short) (srWindow.Left - left);
            srWindow.Left = (short) left;
            srWindow.Top = (short) top;

            bool r = Win32Native.SetConsoleWindowInfo(ConsoleOutputHandle, true, &srWindow);
            if (!r)
                __Error.WinIOError();
        }

        public static int CursorLeft
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
                return csbi.dwCursorPosition.X;
            }
            [ResourceExposure(ResourceScope.Process)] [ResourceConsumption(ResourceScope.Process)] set { SetCursorPosition(value, CursorTop); }
        }

        public static int CursorTop
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
                return csbi.dwCursorPosition.Y;
            }
            [ResourceExposure(ResourceScope.Process)] [ResourceConsumption(ResourceScope.Process)] set { SetCursorPosition(CursorLeft, value); }
        }

        [SecuritySafeCritical] // auto-generated
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static void SetCursorPosition(int left, int top)
        {
            // Note on argument checking - the upper bounds are NOT correct 
            // here!  But it looks slightly expensive to compute them.  Let
            // Windows calculate them, then we'll give a nice error message.
            if (left < 0 || left >= short.MaxValue)
                throw new ArgumentOutOfRangeException(paramName: "left", actualValue: left, message: Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
            if (top < 0 || top >= short.MaxValue)
                throw new ArgumentOutOfRangeException(paramName: "top", actualValue: top, message: Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
            Contract.EndContractBlock();

            new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();

            var hConsole = ConsoleOutputHandle;
            Win32Native.COORD coords = new Win32Native.COORD();
            coords.X = (short) left;
            coords.Y = (short) top;
            bool r = Win32Native.SetConsoleCursorPosition(hConsole, coords);
            if (!r)
            {
                // Give a nice error message for out of range sizes
                var errorCode = Marshal.GetLastWin32Error();
                Win32Native.CONSOLE_SCREEN_BUFFER_INFO csbi = GetBufferInfo();
                if (left < 0 || left >= csbi.dwSize.X)
                    throw new ArgumentOutOfRangeException(paramName: "left", actualValue: left, message: Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));
                if (top < 0 || top >= csbi.dwSize.Y)
                    throw new ArgumentOutOfRangeException(paramName: "top", actualValue: top, message: Environment.GetResourceString("ArgumentOutOfRange_ConsoleBufferBoundaries"));

                __Error.WinIOError(errorCode, string.Empty);
            }
        }

        public static int CursorSize
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                Win32Native.CONSOLE_CURSOR_INFO cci;
                var hConsole = ConsoleOutputHandle;
                bool r = Win32Native.GetConsoleCursorInfo(hConsole, out cci);
                if (!r)
                    __Error.WinIOError();

                return cci.dwSize;
            }
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            set
            {
                // Value should be a percentage from [1, 100].
                if (value < 1 || value > 100)
                    throw new ArgumentOutOfRangeException(paramName: "value", actualValue: value, message: Environment.GetResourceString("ArgumentOutOfRange_CursorSize"));
                Contract.EndContractBlock();

                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();

                Win32Native.CONSOLE_CURSOR_INFO cci;
                var hConsole = ConsoleOutputHandle;
                bool r = Win32Native.GetConsoleCursorInfo(hConsole, out cci);
                if (!r)
                    __Error.WinIOError();

                cci.dwSize = value;
                r = Win32Native.SetConsoleCursorInfo(hConsole, ref cci);
                if (!r)
                    __Error.WinIOError();
            }
        }

        public static bool CursorVisible
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                Win32Native.CONSOLE_CURSOR_INFO cci;
                var hConsole = ConsoleOutputHandle;
                bool r = Win32Native.GetConsoleCursorInfo(hConsole, out cci);
                if (!r)
                    __Error.WinIOError();

                return cci.bVisible;
            }
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            set
            {
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();

                Win32Native.CONSOLE_CURSOR_INFO cci;
                var hConsole = ConsoleOutputHandle;
                bool r = Win32Native.GetConsoleCursorInfo(hConsole, out cci);
                if (!r)
                    __Error.WinIOError();

                cci.bVisible = value;
                r = Win32Native.SetConsoleCursorInfo(hConsole, ref cci);
                if (!r)
                    __Error.WinIOError();
            }
        }

#if !FEATURE_CORECLR
        [SecurityCritical]
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Ansi)]
        [ResourceExposure(ResourceScope.None)]
        [SuppressUnmanagedCodeSecurity]
        static extern int GetTitleNative(StringHandleOnStack outTitle, out int outTitleLength);

        public static string Title
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                string title = null;
                var titleLength = -1;
                var r = GetTitleNative(JitHelpers.GetStringHandleOnStack(ref title), out titleLength);

                if (0 != r)
                    __Error.WinIOError(r, string.Empty);

                if (titleLength > MaxConsoleTitleLength)
                    throw new InvalidOperationException(Environment.GetResourceString("ArgumentOutOfRange_ConsoleTitleTooLong"));

                Contract.Assert(title.Length == titleLength);

                return title;
            }

            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            set
            {
                if (value == null)
                    throw new ArgumentNullException(paramName: "value");
                if (value.Length > MaxConsoleTitleLength)
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_ConsoleTitleTooLong"));
                Contract.EndContractBlock();

                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();

                if (!Win32Native.SetConsoleTitle(value))
                    __Error.WinIOError();
            }
        }
#endif // !FEATURE_CORECLR

        [Flags]
        internal enum ControlKeyState
        {
            RightAltPressed = 0x0001,
            LeftAltPressed = 0x0002,
            RightCtrlPressed = 0x0004,
            LeftCtrlPressed = 0x0008,
            ShiftPressed = 0x0010,
            NumLockOn = 0x0020,
            ScrollLockOn = 0x0040,
            CapsLockOn = 0x0080,
            EnhancedKey = 0x0100
        }

        [HostProtection(UI = true)]
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static ConsoleKeyInfo ReadKey() => ReadKey(false);

        // For tracking Alt+NumPad unicode key sequence. When you press Alt key down 
        // and press a numpad unicode decimal sequence and then release Alt key, the
        // desired effect is to translate the sequence into one Unicode KeyPress. 
        // We need to keep track of the Alt+NumPad sequence and surface the final
        // unicode char alone when the Alt key is released. 
        [SecurityCritical] // auto-generated
        static bool IsAltKeyDown(Win32Native.InputRecord ir) => ((ControlKeyState) ir.keyEvent.controlKeyState
                                                                 & (ControlKeyState.LeftAltPressed | ControlKeyState.RightAltPressed)) != 0;

        // Skip non key events. Generally we want to surface only KeyDown event 
        // and suppress KeyUp event from the same Key press but there are cases
        // where the assumption of KeyDown-KeyUp pairing for a given key press 
        // is invalid. For example in IME Unicode keyboard input, we often see
        // only KeyUp until the key is released.  
        [SecurityCritical] // auto-generated
        static bool IsKeyDownEvent(Win32Native.InputRecord ir) => ir.eventType == Win32Native.KEY_EVENT && ir.keyEvent.keyDown;

        [SecurityCritical] // auto-generated
        static bool IsModKey(Win32Native.InputRecord ir)
        {
            // We should also skip over Shift, Control, and Alt, as well as caps lock.
            // Apparently we don't need to check for 0xA0 through 0xA5, which are keys like 
            // Left Control & Right Control. See the ConsoleKey enum for these values.
            short keyCode = ir.keyEvent.virtualKeyCode;
            return keyCode >= 0x10 && keyCode <= 0x12
                   || keyCode == 0x14 || keyCode == 0x90 || keyCode == 0x91;
        }

        [SecuritySafeCritical] // auto-generated
        [HostProtection(UI = true)]
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static ConsoleKeyInfo ReadKey(bool intercept)
        {
            Win32Native.InputRecord ir;
            var numEventsRead = -1;
            bool r;

            lock (ReadKeySyncObject)
            {
                if (_cachedInputRecord.eventType == Win32Native.KEY_EVENT)
                {
                    // We had a previous keystroke with repeated characters.
                    ir = _cachedInputRecord;
                    if (_cachedInputRecord.keyEvent.repeatCount == 0)
                        _cachedInputRecord.eventType = -1;
                    else
                        _cachedInputRecord.keyEvent.repeatCount--;
                    // We will return one key from this method, so we decrement the
                    // repeatCount here, leaving the cachedInputRecord in the "queue".
                }
                else
                { // We did NOT have a previous keystroke with repeated characters:

                    while (true)
                    {
                        r = Win32Native.ReadConsoleInput(ConsoleInputHandle, out ir, 1, out numEventsRead);
                        if (!r || numEventsRead == 0)
                        {
                            // This will fail when stdin is redirected from a file or pipe. 
                            // We could theoretically call Console.Read here, but I 
                            // think we might do some things incorrectly then.
                            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ConsoleReadKeyOnFile"));
                        }

                        short keyCode = ir.keyEvent.virtualKeyCode;

                        // First check for non-keyboard events & discard them. Generally we tap into only KeyDown events and ignore the KeyUp events
                        // but it is possible that we are dealing with a Alt+NumPad unicode key sequence, the final unicode char is revealed only when 
                        // the Alt key is released (i.e when the sequence is complete). To avoid noise, when the Alt key is down, we should eat up 
                        // any intermediate key strokes (from NumPad) that collectively forms the Unicode character.  

                        if (!IsKeyDownEvent(ir))
                        {
                            // 
                            if (keyCode != AltVKCode)
                                continue;
                        }

                        var ch = (char) ir.keyEvent.uChar;

                        // In a Alt+NumPad unicode sequence, when the alt key is released uChar will represent the final unicode character, we need to 
                        // surface this. VirtualKeyCode for this event will be Alt from the Alt-Up key event. This is probably not the right code, 
                        // especially when we don't expose ConsoleKey.Alt, so this will end up being the hex value (0x12). VK_PACKET comes very 
                        // close to being useful and something that we could look into using for this purpose... 

                        if (ch == 0)
                        {
                            // Skip mod keys.
                            if (IsModKey(ir))
                                continue;
                        }

                        // When Alt is down, it is possible that we are in the middle of a Alt+NumPad unicode sequence.
                        // Escape any intermediate NumPad keys whether NumLock is on or not (notepad behavior)
                        var key = (ConsoleKey) keyCode;
                        if (IsAltKeyDown(ir) && (key >= ConsoleKey.NumPad0 && key <= ConsoleKey.NumPad9
                                                 || key == ConsoleKey.Clear || key == ConsoleKey.Insert
                                                 || key >= ConsoleKey.PageUp && key <= ConsoleKey.DownArrow))
                            continue;

                        if (ir.keyEvent.repeatCount > 1)
                        {
                            ir.keyEvent.repeatCount--;
                            _cachedInputRecord = ir;
                        }
                        break;
                    }
                } // we did NOT have a previous keystroke with repeated characters.
            } // lock(ReadKeySyncObject)

            var state = (ControlKeyState) ir.keyEvent.controlKeyState;
            var shift = (state & ControlKeyState.ShiftPressed) != 0;
            var alt = (state & (ControlKeyState.LeftAltPressed | ControlKeyState.RightAltPressed)) != 0;
            var control = (state & (ControlKeyState.LeftCtrlPressed | ControlKeyState.RightCtrlPressed)) != 0;

            var info = new ConsoleKeyInfo((char) ir.keyEvent.uChar, (ConsoleKey) ir.keyEvent.virtualKeyCode, shift, alt, control);

            if (!intercept)
                Console.Write(ir.keyEvent.uChar);
            return info;
        } // public static ConsoleKeyInfo ReadKey(bool intercept)

        public static bool KeyAvailable
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            [HostProtection(UI = true)]
            get
            {
                if (_cachedInputRecord.eventType == Win32Native.KEY_EVENT)
                    return true;

                Win32Native.InputRecord ir = new Win32Native.InputRecord();
                var numEventsRead = 0;
                while (true)
                {
                    bool r = Win32Native.PeekConsoleInput(ConsoleInputHandle, out ir, 1, out numEventsRead);
                    if (!r)
                    {
                        var errorCode = Marshal.GetLastWin32Error();
                        if (errorCode == Win32Native.ERROR_INVALID_HANDLE)
                            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ConsoleKeyAvailableOnFile"));
                        __Error.WinIOError(errorCode, "stdin");
                    }

                    if (numEventsRead == 0)
                        return false;

                    // Skip non key-down && mod key events.
                    if (!IsKeyDownEvent(ir) || IsModKey(ir))
                    {
                        // 

                        // Exempt Alt keyUp for possible Alt+NumPad unicode sequence.
                        //short keyCode = ir.keyEvent.virtualKeyCode;
                        //if (!IsKeyDownEvent(ir) && (keyCode == AltVKCode))
                        //    return true;

                        r = Win32Native.ReadConsoleInput(ConsoleInputHandle, out ir, 1, out numEventsRead);

                        if (!r)
                            __Error.WinIOError();
                    }
                    else
                        return true;
                }
            } // get
        } // public static bool KeyAvailable

        public static bool NumberLock
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            get
            {
                short s = Win32Native.GetKeyState(NumberLockVKCode);
                return (s & 1) == 1;
            }
        }

        public static bool CapsLock
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            get
            {
                short s = Win32Native.GetKeyState(CapsLockVKCode);
                return (s & 1) == 1;
            }
        }

        public static bool TreatControlCAsInput
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get
            {
                var handle = ConsoleInputHandle;
                if (handle == Win32Native.INVALID_HANDLE_VALUE)
                    throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));
                var mode = 0;
                bool r = Win32Native.GetConsoleMode(handle, out mode);
                if (!r)
                    __Error.WinIOError();
                return (mode & Win32Native.ENABLE_PROCESSED_INPUT) == 0;
            }
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            set
            {
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();

                var handle = ConsoleInputHandle;
                if (handle == Win32Native.INVALID_HANDLE_VALUE)
                    throw new IOException(Environment.GetResourceString("IO.IO_NoConsole"));

                var mode = 0;
                bool r = Win32Native.GetConsoleMode(handle, out mode);
                if (value)
                    mode &= ~Win32Native.ENABLE_PROCESSED_INPUT;
                else
                    mode |= Win32Native.ENABLE_PROCESSED_INPUT;
                r = Win32Native.SetConsoleMode(handle, mode);

                if (!r)
                    __Error.WinIOError();
            }
        }
#endif // !FEATURE_PAL

        // During an appdomain unload, we must call into the OS and remove
        // our delegate from the OS's list of console control handlers.  If
        // we don't do this, the OS will call back on a delegate that no
        // longer exists.  So, subclass CriticalFinalizableObject.
        // This problem would theoretically exist during process exit for a
        // single appdomain too, so using a critical finalizer is probably
        // better than the appdomain unload event (I'm not sure we call that
        // in the default appdomain during process exit).
        internal sealed class ControlCHooker : CriticalFinalizerObject
        {
            [SecurityCritical] // auto-generated
            readonly Win32Native.ConsoleCtrlHandlerRoutine _handler;

            bool _hooked;

            [SecurityCritical] // auto-generated
            internal ControlCHooker()
            {
                _handler = new Win32Native.ConsoleCtrlHandlerRoutine(BreakEvent);
            }

            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            ~ControlCHooker()
            {
                Unhook();
            }

            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            internal void Hook()
            {
                if (!_hooked)
                {
                    bool r = Win32Native.SetConsoleCtrlHandler(_handler, true);
                    if (!r)
                        __Error.WinIOError();
                    _hooked = true;
                }
            }

            [SecuritySafeCritical] // auto-generated
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            internal void Unhook()
            {
                if (_hooked)
                {
                    bool r = Win32Native.SetConsoleCtrlHandler(_handler, false);
                    if (!r)
                        __Error.WinIOError();
                    _hooked = false;
                }
            }
        } // internal sealed class ControlCHooker

        // A class with data so ControlC handlers can be called on a Threadpool thread.
        sealed class ControlCDelegateData
        {
            internal readonly ConsoleSpecialKey ControlKey;
            internal readonly ManualResetEvent CompletionEvent;
            internal readonly ConsoleCancelEventHandler CancelCallbacks;
            internal bool Cancel;
            internal bool DelegateStarted;

            internal ControlCDelegateData(ConsoleSpecialKey controlKey, ConsoleCancelEventHandler cancelCallbacks)
            {
                ControlKey = controlKey;
                CancelCallbacks = cancelCallbacks;
                CompletionEvent = new ManualResetEvent(false);
                // this.Cancel defaults to false
                // this.DelegateStarted defaults to false
            }
        }

        // Returns true if we've "handled" the break request, false if
        // we want to terminate the process (or at least let the next
        // control handler function have a chance).
        static bool BreakEvent(int controlType)
        {
            // The thread that this gets called back on has a very small stack on 64 bit systems. There is
            // not enough space to handle a managed exception being caught and thrown. So, queue up a work
            // item on another thread for the actual event callback.

            if (controlType == Win32Native.CTRL_C_EVENT ||
                controlType == Win32Native.CTRL_BREAK_EVENT)
            {
                // To avoid ---- between remove handler and raising the event
                var cancelCallbacks = _cancelCallbacks;
                if (cancelCallbacks == null)
                    return false;

                // Create the delegate
                var controlKey = controlType == 0 ? ConsoleSpecialKey.ControlC : ConsoleSpecialKey.ControlBreak;
                var delegateData = new ControlCDelegateData(controlKey, cancelCallbacks);
                WaitCallback controlCCallback = ControlCDelegate;

                // Queue the delegate
                if (!ThreadPool.QueueUserWorkItem(controlCCallback, delegateData))
                {
                    Contract.Assert(false, userMessage: "ThreadPool.QueueUserWorkItem returned false without throwing. Unable to execute ControlC handler");
                    return false;
                }
                // Block until the delegate is done. We need to be robust in the face of the work item not executing
                // but we also want to get control back immediately after it is done and we don't want to give the
                // handler a fixed time limit in case it needs to display UI. Wait on the event twice, once with a
                // timout and a second time without if we are sure that the handler actually started.
                var controlCWaitTime = new TimeSpan(0, 0, 30); // 30 seconds
                delegateData.CompletionEvent.WaitOne(controlCWaitTime, false);
                if (!delegateData.DelegateStarted)
                {
                    Contract.Assert(false, userMessage: "ThreadPool.QueueUserWorkItem did not execute the handler within 30 seconds.");
                    return false;
                }
                delegateData.CompletionEvent.WaitOne();
                delegateData.CompletionEvent.Close();
                return delegateData.Cancel;
            }
            return false;
        }

        // This is the worker delegate that is called on the Threadpool thread to fire the actual events. It must guarantee that it
        // signals the caller on the ControlC thread so that it does not block indefinitely.
        static void ControlCDelegate(object data)
        {
            var controlCData = (ControlCDelegateData) data;
            try
            {
                controlCData.DelegateStarted = true;
                var args = new ConsoleCancelEventArgs(controlCData.ControlKey);
                controlCData.CancelCallbacks(null, args);
                controlCData.Cancel = args.Cancel;
            }
            finally
            {
                controlCData.CompletionEvent.Set();
            }
        }

        // Note: hooking this event allows you to prevent Control-C from 
        // killing a console app, which is somewhat surprising for users.
        // Some permission seems appropriate.  We chose UI permission for lack
        // of a better one.  However, we also applied host protection 
        // permission here as well, for self-affecting process management.
        // This allows hosts to prevent people from adding a handler for
        // this event.
        public static event ConsoleCancelEventHandler CancelKeyPress
        {
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            add
            {
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();

                lock (InternalSyncObject)
                {
                    // Add this delegate to the pile.
                    _cancelCallbacks += value;

                    // If we haven't registered our control-C handler, do it.
                    if (_hooker == null)
                    {
                        _hooker = new ControlCHooker();
                        _hooker.Hook();
                    }
                }
            }
            [SecuritySafeCritical] // auto-generated
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            remove
            {
                new UIPermission(UIPermissionWindow.SafeTopLevelWindows).Demand();

                lock (InternalSyncObject)
                {
                    // If count was 0, call SetConsoleCtrlEvent to remove cb.
                    _cancelCallbacks -= value;
                    Contract.Assert(_cancelCallbacks == null || _cancelCallbacks.GetInvocationList().Length > 0, userMessage: "Teach Console::CancelKeyPress to handle a non-null but empty list of callbacks");
                    if (_hooker != null && _cancelCallbacks == null)
                        _hooker.Unhook();
                }
            }
        }

        [HostProtection(UI = true)]
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static Stream OpenStandardError() => OpenStandardError(DefaultConsoleBufferSize);

#if FEATURE_CORECLR
        [System.Security.SecurityCritical] // auto-generated
#endif
        [HostProtection(UI = true)]
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static Stream OpenStandardError(int bufferSize)
        {
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            Contract.EndContractBlock();
            return GetStandardFile(Win32Native.STD_ERROR_HANDLE,
                                   FileAccess.Write,
                                   bufferSize);
        }

        [HostProtection(UI = true)]
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static Stream OpenStandardInput() => OpenStandardInput(DefaultConsoleBufferSize);

#if FEATURE_CORECLR
        [System.Security.SecurityCritical] // auto-generated
#endif
        [HostProtection(UI = true)]
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static Stream OpenStandardInput(int bufferSize)
        {
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            Contract.EndContractBlock();
            return GetStandardFile(Win32Native.STD_INPUT_HANDLE,
                                   FileAccess.Read,
                                   bufferSize);
        }

        [HostProtection(UI = true)]
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static Stream OpenStandardOutput() => OpenStandardOutput(DefaultConsoleBufferSize);

#if FEATURE_CORECLR
        [System.Security.SecurityCritical] // auto-generated
#endif
        [HostProtection(UI = true)]
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static Stream OpenStandardOutput(int bufferSize)
        {
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            Contract.EndContractBlock();
            return GetStandardFile(Win32Native.STD_OUTPUT_HANDLE,
                                   FileAccess.Write,
                                   bufferSize);
        }

#if FEATURE_CORECLR
        [System.Security.SecurityCritical] // auto-generated
#else
        [SecuritySafeCritical]
#endif
        [HostProtection(UI = true)]
        [ResourceExposure(ResourceScope.AppDomain)]
        public static void SetIn(TextReader newIn)
        {
            if (newIn == null)
                throw new ArgumentNullException(paramName: "newIn");
            Contract.EndContractBlock();
#pragma warning disable 618
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
#pragma warning restore 618

            newIn = TextReader.Synchronized(newIn);
            lock (InternalSyncObject)
            {
                _in = newIn;
            }
        }

#if FEATURE_CORECLR
        [System.Security.SecurityCritical] // auto-generated
#else
        [SecuritySafeCritical]
#endif
        [HostProtection(UI = true)]
        [ResourceExposure(ResourceScope.AppDomain)]
        public static void SetOut(TextWriter newOut)
        {
            if (newOut == null)
                throw new ArgumentNullException(paramName: "newOut");
            Contract.EndContractBlock();
#pragma warning disable 618
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
#pragma warning restore 618
#if FEATURE_CODEPAGES_FILE // if no codepages file then we are locked into default codepage  and this field is not used              
            _isOutTextWriterRedirected = true;
#endif
            newOut = TextWriter.Synchronized(newOut);
            lock (InternalSyncObject)
            {
                _out = newOut;
            }
        }

#if FEATURE_CORECLR
        [System.Security.SecurityCritical] // auto-generated
#else
        [SecuritySafeCritical]
#endif
        [HostProtection(UI = true)]
        [ResourceExposure(ResourceScope.AppDomain)]
        public static void SetError(TextWriter newError)
        {
            if (newError == null)
                throw new ArgumentNullException(paramName: "newError");
            Contract.EndContractBlock();
#pragma warning disable 618
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
#pragma warning restore 618
#if FEATURE_CODEPAGES_FILE // if no codepages file then we are locked into default codepage  and this field is not used              
            _isErrorTextWriterRedirected = true;
#endif
            newError = TextWriter.Synchronized(newError);
            lock (InternalSyncObject)
            {
                _error = newError;
            }
        }

        //
        // Give a hint to the code generator to not inline the common console methods. The console methods are 
        // not performance critical. It is unnecessary code bloat to have them inlined.
        //
        // Moreover, simple repros for codegen bugs are often console-based. It is tedious to manually filter out 
        // the inlined console writelines from them.
        //
        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Read() => In.Read();

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string ReadLine() => In.ReadLine();

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine()
        {
            Out.WriteLine();
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(bool value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(char value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(char[] buffer)
        {
            Out.WriteLine(buffer);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(char[] buffer, int index, int count)
        {
            Out.WriteLine(buffer, index, count);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(decimal value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(double value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(float value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(int value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(UI = true)]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(uint value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(long value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(UI = true)]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(ulong value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(object value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(string value)
        {
            Out.WriteLine(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(string format, object arg0)
        {
            Out.WriteLine(format, arg0);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(string format, object arg0, object arg1)
        {
            Out.WriteLine(format, arg0, arg1);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            Out.WriteLine(format, arg0, arg1, arg2);
        }

        [HostProtection(UI = true)]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(string format, object arg0, object arg1, object arg2, object arg3, __arglist)
        {
            object[] objArgs;
            int argCount;

            ArgIterator args = new ArgIterator(__arglist);

            //+4 to account for the 4 hard-coded arguments at the beginning of the list.
            argCount = args.GetRemainingCount() + 4;

            objArgs = new object[argCount];

            //Handle the hard-coded arguments
            objArgs[0] = arg0;
            objArgs[1] = arg1;
            objArgs[2] = arg2;
            objArgs[3] = arg3;

            //Walk all of the args in the variable part of the argument list.
            for (var i = 4; i < argCount; i++)
                objArgs[i] = TypedReference.ToObject(args.GetNextArg());

            Out.WriteLine(format, objArgs);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(string format, params object[] arg)
        {
            if (arg == null) // avoid ArgumentNullException from String.Format
                Out.WriteLine(format, null, null); // faster than Out.WriteLine(format, (Object)arg);
            else
                Out.WriteLine(format, arg);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(string format, object arg0)
        {
            Out.Write(format, arg0);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(string format, object arg0, object arg1)
        {
            Out.Write(format, arg0, arg1);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(string format, object arg0, object arg1, object arg2)
        {
            Out.Write(format, arg0, arg1, arg2);
        }

        [HostProtection(UI = true)]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(string format, object arg0, object arg1, object arg2, object arg3, __arglist)
        {
            object[] objArgs;
            int argCount;

            ArgIterator args = new ArgIterator(__arglist);

            //+4 to account for the 4 hard-coded arguments at the beginning of the list.
            argCount = args.GetRemainingCount() + 4;

            objArgs = new object[argCount];

            //Handle the hard-coded arguments
            objArgs[0] = arg0;
            objArgs[1] = arg1;
            objArgs[2] = arg2;
            objArgs[3] = arg3;

            //Walk all of the args in the variable part of the argument list.
            for (var i = 4; i < argCount; i++)
                objArgs[i] = TypedReference.ToObject(args.GetNextArg());

            Out.Write(format, objArgs);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(string format, params object[] arg)
        {
            if (arg == null) // avoid ArgumentNullException from String.Format
                Out.Write(format, null, null); // faster than Out.Write(format, (Object)arg);
            else
                Out.Write(format, arg);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(bool value)
        {
            Out.Write(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(char value)
        {
            Out.Write(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(char[] buffer)
        {
            Out.Write(buffer);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(char[] buffer, int index, int count)
        {
            Out.Write(buffer, index, count);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(double value)
        {
            Out.Write(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(decimal value)
        {
            Out.Write(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(float value)
        {
            Out.Write(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(int value)
        {
            Out.Write(value);
        }

        [HostProtection(UI = true)]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(uint value)
        {
            Out.Write(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(long value)
        {
            Out.Write(value);
        }

        [HostProtection(UI = true)]
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(ulong value)
        {
            Out.Write(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(object value)
        {
            Out.Write(value);
        }

        [HostProtection(UI = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(string value)
        {
            Out.Write(value);
        }
    } // public static class Console
} // namespace System