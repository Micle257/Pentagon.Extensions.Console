// -----------------------------------------------------------------------
//  <copyright file="ConsoleWindow.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Buffers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using JetBrains.Annotations;
    using Native;
    using Native.Flags;
    using Native.Structures;
    using Structures;

    /// <summary> Represent a win32 console window. </summary>
    public class ConsoleWindow
    {
        public static ConsoleWindow CurrentWindow;
        readonly IScreenProvider _screenProvider;
        WindowStyle StyleFlags;

        int scrollLength;

        string _title;

        bool _sizeable;

        bool _minimaze;
        InputListener _inputLisnener;

        public ConsoleWindow(IScreenProvider screenProvider)
        {
            _screenProvider = screenProvider;

            if (CurrentWindow == null)
                CurrentWindow = this;
        }

        public event EventHandler<ConsoleKeyInfo> KeyPressed;

        /// <summary> Gets the handle to the window represented by the implementer. </summary>
        public IntPtr Handle => KernelNativeMethods.GetConsoleWindowHandle();

        public IScreen CurrentScreen => Screens.Current;

        public Cursor Cursor => CurrentScreen.Cursor;

        [NotNull]
        public ISelectable<IScreen> Screens { get; } = new ScreenContainer();

        //public ConsoleFont[] ConsoleFonts TODO
        //{
        //    get
        //    {
        //        var fonts = new ConsoleFont[KernelNativeMethods.GetNumberOfConsoleFonts()];
        //        if (fonts.Length > 0)
        //            KernelNativeMethods.GetConsoleFontInfo(OutputStdHandle, false, (uint) fonts.Length, fonts);
        //        return fonts;
        //    }
        //}

        public int Width
        {
            get => Console.WindowWidth;
            set
            {
                if (value < Console.LargestWindowWidth)
                    Console.WindowWidth = value;
            }
        }

        public int Height
        {
            get => Console.WindowHeight;
            set
            {
                if (value < Console.LargestWindowHeight)
                    Console.WindowHeight = value;
            }
        }

        /// <summary> Gets or sets the title of the console window. </summary>
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                Console.Title = _title;
            }
        }

        public bool Sizeable
        {
            get => _sizeable;
            set
            {
                EnsureWindowStyle();
                _sizeable = value;
                if (value)
                    StyleFlags = StyleFlags | WindowStyle.MaximizeBox | WindowStyle.SizeBox;
                else
                    StyleFlags = StyleFlags & ~(WindowStyle.MaximizeBox | WindowStyle.SizeBox);

                NativeMethods.SetWindowLongPtr(Handle, NativeMethods.GWL_STYLE, StyleFlags);
            }
        }

        //public bool Minimaze
        //{
        //    get => _minimaze;
        //    set
        //    {
        //        _minimaze = value;
        //        StyleFlags = value ? StyleFlags | WindowStyle.MinimizeBox | WindowStyle.Sysmenu : StyleFlags & ~WindowStyle.MinimizeBox;
        //        NativeMethods.SetWindowLong(Handle, NativeMethods.GWL_STYLE, StyleFlags);
        //    }
        //}

        public ConsoleContext Context { get; set; }

        //TODO  uint ConsoleFontsCount => KernelNativeMethods.GetNumberOfConsoleFonts();

        IntPtr InputStdHandle => KernelNativeMethods.GetStdHandle(StdHandle.InputHandle);

        IntPtr OutputStdHandle
        {
            get
            {
                var outputStdHandle = KernelNativeMethods.GetStdHandle(StdHandle.OutputHandle);
                return outputStdHandle;
            }
        }

        //TODO  ConsoleMode ConsoleModeInput => KernelNativeMethods.GetConsoleMode(InputStdHandle);

        public static void SetFocusWindow(IntPtr hWnd) => NativeMethods.SetForegroundWindow(hWnd);

        public void AddScreen(IScreen screen)
        {
            Screens.AddItem(screen);
        }
        
        /// <summary> Sets the size of the window and buffer. </summary>
        /// <param name="wWidth"> Window Width. </param>
        /// <param name="wHeight"> Window Height. </param>
        /// <param name="bWidth"> Buffer Width. </param>
        /// <param name="bHeight"> Buffer Height. </param>
        public void SetSize(int wWidth, int wHeight)
        {
            Width = wWidth;
            Height = wHeight;
        }

        /// <summary> Sets the window position in pixels. </summary>
        //TODO  public void SetWindowPos(int x, int y) => NativeMethods.SetWindowPos(Handle, 0, x, y, 0, 0, 0x05);
        public void ScrollDown()
        {
            if (Height == CurrentScreen.Height)
                return;
            var curpos = CurrentScreen.Cursor.Y;
            CurrentScreen.Cursor.Coord = new BufferPoint(CurrentScreen.Cursor.X, Height + scrollLength);
            if (CurrentScreen.Cursor.Y < CurrentScreen.Height)
            {
                CurrentScreen.Cursor.Offset(0, 1);
                scrollLength++;
                CurrentScreen.Cursor.Y = curpos + 1;
            }
            else
                CurrentScreen.Cursor.Y = curpos;
        }

        public void ScrollUp()
        {
            if (Height == CurrentScreen.Height)
                return;
            var curpos = CurrentScreen.Cursor.Y;
            CurrentScreen.Cursor.Coord = new BufferPoint(CurrentScreen.Cursor.X, 1 + scrollLength);
            if (CurrentScreen.Cursor.Y > 1)
            {
                CurrentScreen.Cursor.Offset(0, -1);
                scrollLength--;
                CurrentScreen.Cursor.Y = curpos - 1;
            }
            else
                CurrentScreen.Cursor.Y = curpos;
        }

        public void ShowConsole(bool show)
        {
            //if (show) TODO
            //    NativeMethods.ShowWindow(Handle, 5);
            //else
            //    NativeMethods.ShowWindow(Handle, 0);
        }

        public void CreateConsole()
        {
            KernelNativeMethods.AllocConsole();
            Console.TreatControlCAsInput = true;
            //  SetConsoleMode(OutputStdHandle, ~ConsoleMode.MouseInput | ConsoleMode.ProcessedInput | ConsoleMode.ExtendedFlags);
        }

        /// <summary> Focuses this console window. </summary>
        //TODO  public void Focus() => NativeMethods.SetForegroundWindow(Handle);
        public ConsoleFontInfoEx GetCurrentFont()
        {
            var font = KernelNativeMethods.GetCurrentConsoleFont(OutputStdHandle, false);
            return font;
        }

        //TODO  public void SetCurrentFont(ConsoleFontEx font) => KernelNativeMethods.SetCurrentConsoleFontEx(OutputStdHandle, false, font);

        //TODO  public void SetFont(uint index) => KernelNativeMethods.SetConsoleFont(OutputStdHandle, index);

        // public void SetColor(ConsoleColor consoleColor, FormColor targetColor) => SetColor(consoleColor, targetColor.R, targetColor.G, targetColor.B);

        //TODOpublic void SetColor(ConsoleColor color, uint r, uint g, uint b)
        //{
        //    var hConsoleOutput = OutputStdHandle;
        //    var csbe = KernelNativeMethods.GetConsoleScreenBufferInfoEx(hConsoleOutput);

        //    switch (color)
        //    {
        //        case ConsoleColor.Black:
        //            csbe.black = new ColorRef(r, g, b);
        //            break;
        //        case ConsoleColor.DarkBlue:
        //            csbe.darkBlue = new ColorRef(r, g, b);
        //            break;
        //        case ConsoleColor.DarkGreen:
        //            csbe.darkGreen = new ColorRef(r, g, b);
        //            break;
        //        case ConsoleColor.DarkCyan:
        //            csbe.darkCyan = new ColorRef(r, g, b);
        //            break;
        //        case ConsoleColor.DarkRed:
        //            csbe.darkRed = new ColorRef(r, g, b);
        //            break;
        //        case ConsoleColor.DarkMagenta:
        //            csbe.darkMagenta = new ColorRef(r, g, b);
        //            break;
        //        case ConsoleColor.DarkYellow:
        //            csbe.darkYellow = new ColorRef(r, g, b);
        //            break;
        //        case ConsoleColor.Gray:
        //            csbe.gray = new ColorRef(r, g, b);
        //            break;
        //        case ConsoleColor.DarkGray:
        //            csbe.darkGray = new ColorRef(r, g, b);
        //            break;
        //        case ConsoleColor.Blue:
        //            csbe.blue = new ColorRef(r, g, b);
        //            break;
        //        case ConsoleColor.Green:
        //            csbe.green = new ColorRef(r, g, b);
        //            break;
        //        case ConsoleColor.Cyan:
        //            csbe.cyan = new ColorRef(r, g, b);
        //            break;
        //        case ConsoleColor.Red:
        //            csbe.red = new ColorRef(r, g, b);
        //            break;
        //        case ConsoleColor.Magenta:
        //            csbe.magenta = new ColorRef(r, g, b);
        //            break;
        //        case ConsoleColor.Yellow:
        //            csbe.yellow = new ColorRef(r, g, b);
        //            break;
        //        case ConsoleColor.White:
        //            csbe.white = new ColorRef(r, g, b);
        //            break;
        //    }

        //    ++csbe.srWindow.Bottom;
        //    ++csbe.srWindow.Right;
        //    KernelNativeMethods.SetConsoleScreenBufferInfo(hConsoleOutput, ref csbe);
        //}
        public Task RunInputListener()
        {
            if (!_inputLisnener?.IsRunning != true)
            {
                _inputLisnener = new InputListener();
                _inputLisnener.KeyPressed += (s, e) => KeyPressed?.Invoke(this, e);
                return _inputLisnener.StartListeningAsync();
            }

            throw new InvalidOperationException(message: "Input listener is already running.");
        }

        public Task RunAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            InputListener.Current.KeyPressed += delegate { Screens.Current.Cursor.Show = false; };
            return InputListener.Current.StartListeningAsync(cancellationToken);
        }

        void EnsureWindowStyle()
        {
            if (StyleFlags == 0)
                StyleFlags = NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_STYLE);
        }

        public void SelectScreen(IScreen screen)
        {
            Screens.Select(screen);
        }
    }
}