// -----------------------------------------------------------------------
//  <copyright file="Input.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Controls.Inputs
{
    using System;
    using Buffers;
    using Helpers;
    using Structures;

    public class Input
    {
        readonly bool m_curShow;
        readonly int m_curSize;
        int mTextPos;
        SelectMode mModeSel;
        int mSel1;

        public Input(InputType type, InputShowStyle style = 0, int charLimit = 0)
        {
            Current = this;
            Type = type;
            Coord = Window.Cursor.Coord;
            m_curShow = Window.Cursor.Show;
            m_curSize = Window.Cursor.Size;
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
        public bool IsActive { get; set; } = true;

        public BufferPoint Coord { get; set; }
        public bool ShowCursor { get; set; } = true;

        public string InputText { get; private set; } = "";
        public ConsoleColour InputColor { get; set; } = ConsoleColour.Text;
        public ConsoleColour SelectColor { get; set; } = ConsoleColour.InvertText;
        public char PassChar { get; set; } = '*';
        public bool WriteLineAfter { get; set; }
        public bool AllowMovement { get; set; } = true;
        public bool AllowSelect { get; set; } = true;

        public bool EmptyEntry { get; set; }
        public ConsoleKeyInfo CurrentKeyHit { get; internal set; }
        static ConsoleWindow Window => ConsoleWindow.CurrentWindow;

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
            Window.Cursor.Coord = Coord;
            while (IsActive)
                inputLoop();
            return InputText;
        }

        public string RunOnce()
        {
            Window.Cursor.Coord = Coord;
            if (IsActive)
                inputLoop();
            return InputText;
        }

        void inputLoop()
        {
            Window.Cursor.X = Coord.X + mTextPos;
            Window.Cursor.Y = Coord.Y;
            Window.Cursor.Show = ShowCursor;
            Window.Cursor.Size = CursorSize;

            CurrentKeyHit = WaitKey();

            if (CurrentKeyHit.KeyChar == (char) ConsoleKey.Enter)
            {
                IsActive = false;
                if (InputText.Length == 0)
                {
                    Inputless?.Invoke(this, null);
                    IsActive |= !EmptyEntry;
                }
                Confirming?.Invoke(this, null);
                Window.Cursor.Show = m_curShow;
                Window.Cursor.Size = m_curSize;
            }
            else if (CurrentKeyHit.KeyChar == (char) ConsoleKey.Backspace)
            {
                if (InputText.Length > 0 && (mTextPos > 0 || mModeSel != 0))
                {
                    mText.Clear();
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
                            mText.Print();
                            break;
                        case InputShowStyle.PeekPassword:
                        case InputShowStyle.Password:
                            mPassText.Print();
                            break;
                    }
                }
            }
            else if (CurrentKeyHit.Key == ConsoleKey.Delete)
            {
                if (InputText.Length > 0 && mTextPos < InputText.Length)
                {
                    mText.Clear();
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
                            mText.Print();
                            break;
                        case InputShowStyle.PeekPassword:
                        case InputShowStyle.Password:
                            mPassText.Print();
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
                    InputText = null;
                if (WriteLineAfter)
                    Window.Cursor.Coord = new BufferPoint(1, Window.Cursor.Y + 1);
                else
                    Window.Cursor.Coord = new BufferPoint(Window.Cursor.X, 0);
                Window.Cursor.Show = m_curShow;
                Window.Cursor.Size = m_curSize;
            }
            else if (CharacterLimit != 0 && InputText.Length >= CharacterLimit)
                LimitReached?.Invoke(this, null);
            else if (AllowMovement && CurrentKeyHit.Key == ConsoleKey.LeftArrow)
            {
                if (mTextPos > 0)
                {
                    if (CurrentKeyHit.Modifiers == ConsoleModifiers.Shift && AllowSelect && AllowMovement)
                        SelectLeft();
                    else
                    {
                        if (mModeSel != 0)
                        {
                            CancelSelect();
                            mText.Clear();
                            mText.Print();
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
                    if (CurrentKeyHit.Modifiers == ConsoleModifiers.Shift && AllowSelect && AllowMovement)
                        SelectRight();
                    else
                    {
                        if (mModeSel != 0)
                        {
                            mTextPos = mSel1;
                            CancelSelect();
                            mText.Clear();
                            mText.Print();
                        }
                        else
                            mTextPos++;
                    }
                }
            }
            else if (CurrentKeyHit.Key == ConsoleKey.C && CurrentKeyHit.Modifiers == ConsoleModifiers.Control)
            {
                if (mModeSel != SelectMode.None && ShowStyle == InputShowStyle.None)
                    Clipboard.Copy(mSelect.Data);
            }
            else if (CurrentKeyHit.Key == ConsoleKey.X && CurrentKeyHit.Modifiers == ConsoleModifiers.Control)
            {
                if (mModeSel != SelectMode.None && ShowStyle == InputShowStyle.None)
                {
                    Clipboard.Copy(mSelect.Data);
                    mText.Clear();
                    CancelSelect();
                    InputText = InputText.Remove(mTextPos, mSel1 - mTextPos);
                    mText.Print();
                }
            }
            else if (CurrentKeyHit.Key == ConsoleKey.V && CurrentKeyHit.Modifiers == ConsoleModifiers.Control)
            {
                if (ShowStyle == InputShowStyle.None)
                {
                    mText.Clear();
                    if (mModeSel != SelectMode.None)
                    {
                        CancelSelect();
                        InputText = InputText.Remove(mTextPos, mSel1 - mTextPos);
                    }
                    InputText = InputText.Insert(mTextPos, Clipboard.GetText());
                    mTextPos += Clipboard.GetText().Length;
                    mText.Print();
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
                    mText.Clear();
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
                            mText.Print();
                            break;
                        case InputShowStyle.Password:
                            mPassText.Print();
                            break;
                        case InputShowStyle.PeekPassword:
                            mPassText.Print();
                            Text.Write(CurrentKeyHit.KeyChar, InputColor, Coord.X + mTextPos - 1, Coord.Y);
                            break;
                    }
                }
            }
            if (!IsActive && ShowStyle == InputShowStyle.PeekPassword)
                mPassText.Print();
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
            mText.Print();
            mSelect.Print();
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
            mText.Print();
            mSelect.Print();
        }
    }
}