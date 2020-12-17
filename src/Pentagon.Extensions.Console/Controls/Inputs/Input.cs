// -----------------------------------------------------------------------
//  <copyright file="Input.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Controls.Inputs
{
    using System;
    using Extensions.Console;
    using Extensions.Console.Structures;
    using JetBrains.Annotations;

    class Input
    {
        int mTextPos;
        SelectMode mModeSel;
        int mSel1;

        [NotNull]
        readonly Cursor _initialCursor;

        public Input(InputType type, InputShowStyle style = 0, int charLimit = 0)
        {
            Current = this;
            Type = type;

            _initialCursor = Cursor.Current;
            Coord = _initialCursor.Coord;

            if (Type == InputType.Int)
                CursorSize = 50;
        }

        public event EventHandler Canceling;
        public event EventHandler Confirming;
        public event EventHandler Inputless;
        public event EventHandler LimitReached;

        enum SelectMode
        {
            None,
            Left,
            Right
        }

        public static bool IsCapsLock => Console.CapsLock;

        public static bool IsNumLock => Console.NumberLock;

        public static ConsoleKeyInfo LastUserInput { get; private set; }
        public static Input Current { get; internal set; }

        public InputType Type { get; }
        public InputShowStyle ShowStyle { get; }
        public int CursorSize { get; } = 100;
        public int CharacterLimit { get; } = 0;

        public ConsoleModifiers SelectModifiers { get; set; } = ConsoleModifiers.Control;

        public Func<ConsoleKeyInfo, bool> CopyPredicate { get; set; } = key => key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Alt);

        public Func<ConsoleKeyInfo, bool> PastePredicate { get; set; } = key => key.Key == ConsoleKey.V && key.Modifiers.HasFlag(ConsoleModifiers.Alt);

        public Func<ConsoleKeyInfo, bool> CutPredicate { get; set; } = key => key.Key == ConsoleKey.X && key.Modifiers.HasFlag(ConsoleModifiers.Alt);
        public bool IsActive { get; set; } = true;

        public BufferPoint Coord { get; set; }
        public bool ShowCursor { get; set; } = true;

        [NotNull]
        public string InputText { get; private set; } = "";

        public CliConsoleColor InputColor { get; set; } = CliContext.ColorScheme.Text;
        public CliConsoleColor SelectColor { get; set; } = CliContext.ColorScheme.InvertedText;
        public char PassChar { get; set; } = '*';
        public bool WriteLineAfter { get; set; }
        public bool AllowMovement { get; set; } = true;
        public bool AllowSelect { get; set; } = true;

        public bool AllowEmptyEntry { get; set; }
        public ConsoleKeyInfo CurrentKeyHit { get; internal set; }

        Text mText => new Text(InputText, InputColor, Coord);

        Text mPassText => new Text(new string(PassChar, InputText.Length), InputColor, Coord);

        Text mSelect => new Text(InputText.Substring(mTextPos, mSel1 - mTextPos), SelectColor, new BufferPoint(Coord.X + mTextPos, Coord.Y));

        public static ConsoleKeyInfo WaitKey() => WaitKey(false);

        public static ConsoleKeyInfo WaitKey(bool show)
        {
            LastUserInput = Console.ReadKey(!show);
            return LastUserInput;
        }

        public static void ClearInputBuffer()
        {
            while (Console.KeyAvailable)
                WaitKey();
        }

        public string Run()
        {
            while (IsActive)
                inputLoop();

            return InputText;
        }

        public string RunOnce()
        {
            if (IsActive)
                inputLoop();

            return InputText;
        }

        void inputLoop()
        {
            Cursor.SetCurrent(Coord.X + mTextPos, Coord.Y, ShowCursor, CursorSize);

            CurrentKeyHit = WaitKey();

            if (CurrentKeyHit.KeyChar == (char) ConsoleKey.Enter)
            {
                IsActive = false;

                if (InputText.Length == 0)
                {
                    Inputless?.Invoke(this, null);
                    IsActive = !AllowEmptyEntry;
                }

                Confirming?.Invoke(this, null);

                SetInitialCursor();
            }
            else if (CurrentKeyHit.KeyChar == (char) ConsoleKey.Backspace)
            {
                if (InputText.Length > 0 && (mTextPos > 0 || mModeSel != 0))
                {
                    ConsoleWriter.Clear(mText);
                    if (mModeSel != SelectMode.None)
                    {
                        CancelSelect();
                        InputText = InputText.Remove(mTextPos, mSel1 - mTextPos);
                    }
                    else
                    {
                        InputText = InputText.Remove(mTextPos - 1, 1);
                        mTextPos--;
                    }

                    switch (ShowStyle)
                    {
                        case InputShowStyle.None:
                            ConsoleWriter.Write(mText);
                            break;
                        case InputShowStyle.PeekPassword:
                        case InputShowStyle.Password:
                            ConsoleWriter.Write(mPassText);
                            break;
                    }
                }
            }
            else if (CurrentKeyHit.Key == ConsoleKey.Delete)
            {
                if (InputText.Length > 0 && mTextPos < InputText.Length)
                {
                    ConsoleWriter.Clear(mText);
                    if (mModeSel != SelectMode.None)
                    {
                        CancelSelect();
                        InputText = InputText.Remove(mTextPos, mSel1 - mTextPos);
                    }
                    else
                        InputText = InputText.Remove(mTextPos, 1);

                    switch (ShowStyle)
                    {
                        case InputShowStyle.None:
                            ConsoleWriter.Write(mText);
                            break;
                        case InputShowStyle.PeekPassword:
                        case InputShowStyle.Password:
                            ConsoleWriter.Write(mPassText);
                            break;
                    }
                }
            }
            else if (CurrentKeyHit.Key == ConsoleKey.Home)
                mTextPos = 0;
            else if (CurrentKeyHit.Key == ConsoleKey.End)
                mTextPos = InputText.Length;
            else if (CurrentKeyHit.KeyChar == (char) ConsoleKey.Escape)
            {
                IsActive = false;

                Canceling?.Invoke(this, null);

                if (!IsActive)
                    InputText = "";

                if (WriteLineAfter)
                {
                    Cursor.SetCurrent(c =>
                                      {
                                          c.X = 1;
                                          c.Y = c.Y + 1;
                                      });
                }
                else
                {
                    Cursor.SetCurrent(c => { c.Y = 0; });
                }

                SetInitialCursor();
            }
            else if (CharacterLimit != 0 && InputText.Length >= CharacterLimit)
                LimitReached?.Invoke(this, null);
            else if (AllowMovement && CurrentKeyHit.Key == ConsoleKey.LeftArrow)
            {
                if (mTextPos > 0)
                {
                    if (CurrentKeyHit.Modifiers == SelectModifiers && AllowSelect && AllowMovement)
                        SelectLeft();
                    else
                    {
                        if (mModeSel != 0)
                        {
                            CancelSelect();
                            ConsoleWriter.Clear(mText);
                            ConsoleWriter.Write(mText);
                        }
                        else
                            mTextPos--;
                    }
                }
            }
            else if (AllowMovement && CurrentKeyHit.Key == ConsoleKey.RightArrow)
            {
                if (mTextPos < InputText.Length)
                {
                    if (CurrentKeyHit.Modifiers == SelectModifiers && AllowSelect && AllowMovement)
                        SelectRight();
                    else
                    {
                        if (mModeSel != 0)
                        {
                            mTextPos = mSel1;
                            CancelSelect();
                            ConsoleWriter.Clear(mText);
                            ConsoleWriter.Write(mText);
                        }
                        else
                            mTextPos++;
                    }
                }
            }
            else if (CopyPredicate?.Invoke(CurrentKeyHit) ?? false)
            {
                if (mModeSel != SelectMode.None && ShowStyle == InputShowStyle.None)
                    Clipboard.Copy(mSelect.Data);
            }
            else if (CutPredicate?.Invoke(CurrentKeyHit) ?? false)
            {
                if (mModeSel != SelectMode.None && ShowStyle == InputShowStyle.None)
                {
                    Clipboard.Copy(mSelect.Data);
                    ConsoleWriter.Clear(mText);
                    CancelSelect();
                    InputText = InputText.Remove(mTextPos, mSel1 - mTextPos);
                    ConsoleWriter.Write(mText);
                }
            }
            else if (PastePredicate?.Invoke(CurrentKeyHit) ?? false)
            {
                if (ShowStyle == InputShowStyle.None)
                {
                    ConsoleWriter.Clear(mText);
                    if (mModeSel != SelectMode.None)
                    {
                        CancelSelect();
                        InputText = InputText.Remove(mTextPos, mSel1 - mTextPos);
                    }

                    InputText = InputText.Insert(mTextPos, Clipboard.GetText());
                    mTextPos += Clipboard.GetText().Length;
                    ConsoleWriter.Write(mText);
                }
            }
            else if (CurrentKeyHit.KeyChar == '\t' || CurrentKeyHit.KeyChar < 32 || CurrentKeyHit.KeyChar == 255)
                return;
            else
            {
                if (Type == InputType.String || char.IsDigit(CurrentKeyHit.KeyChar) && Type == InputType.Int)
                {
                    // TODO if (Type == InputType.Int && !IsNumLock)
                    //    MessageBox.Show("NumLock is't enabled.", "Input");
                    ConsoleWriter.Clear(mText);
                    if (mModeSel != 0)
                    {
                        CancelSelect();
                        InputText = InputText.Remove(mTextPos, mSel1 - mTextPos);
                    }

                    InputText = InputText.Insert(mTextPos, CurrentKeyHit.KeyChar.ToString());
                    mTextPos++;
                    switch (ShowStyle)
                    {
                        case InputShowStyle.None:
                            ConsoleWriter.Write(mText);
                            break;
                        case InputShowStyle.Password:
                            ConsoleWriter.Write(mPassText);
                            break;
                        case InputShowStyle.PeekPassword:
                            ConsoleWriter.Write(mPassText);
                            ConsoleWriter.Write(CurrentKeyHit.KeyChar, InputColor, Coord.X + mTextPos - 1, Coord.Y);
                            break;
                    }
                }
            }

            if (!IsActive && ShowStyle == InputShowStyle.PeekPassword)
                ConsoleWriter.Write(mPassText);
        }

        void SetInitialCursor()
        {
            Cursor.SetCurrent(c =>
                              {
                                  c.Show = _initialCursor.Show;
                                  c.Size = _initialCursor.Size;
                              });
        }

        void CancelSelect()
        {
            mModeSel = SelectMode.None;
            ShowCursor = true;
        }

        void SelectLeft()
        {
            if (mModeSel == 0)
            {
                mModeSel = SelectMode.Left;
                ShowCursor = false;
                mSel1 = mTextPos;
                mTextPos--;
            }
            else if (mModeSel == SelectMode.Left)
                mTextPos--;
            else if (mModeSel == SelectMode.Right)
            {
                mSel1--;
                if (mTextPos == mSel1)
                    CancelSelect();
            }

            ConsoleWriter.Write(mText);
            ConsoleWriter.Write(mSelect);
        }

        void SelectRight()
        {
            if (mModeSel == 0)
            {
                mModeSel = SelectMode.Right;
                ShowCursor = false;
                mSel1 = mTextPos + 1;
            }
            else if (mModeSel == SelectMode.Right && InputText.Length != mSel1)
                mSel1++;
            else if (mModeSel == SelectMode.Left)
            {
                mTextPos++;
                if (mTextPos == mSel1)
                    CancelSelect();
            }

            ConsoleWriter.Write(mText);
            ConsoleWriter.Write(mSelect);
        }
    }
}