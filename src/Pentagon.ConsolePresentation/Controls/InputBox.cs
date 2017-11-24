// -----------------------------------------------------------------------
//  <copyright file="InputBox.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Controls
{
    using System;
    using System.Collections.Generic;
    using Buffers;
    using Inputs;
    using Structures;

    public class InputBox : Control
    {
        int _size;
        string _inputText = "";
        int _cursorSize = 25;
        bool _showCursor = true;
        ConsoleColour _inputColor = ConsoleColour.Text;

        string _writtenText;
        BufferPoint _cursorPoint;
        int _currentCaretIndex;

        public InputBox()
        {
            Size = 15;
            CanFocus = true;
            InputListener.Current.KeyPressing += delegate { OnCursorChanged(_cursorSize, _showCursor); };
        }

        public int Size
        {
            get => _size;
            set
            {
                if (_size == value)
                    return;

                _size = value;

                OnSizeChanged(_size, 1);
            }
        }

        public InputType Type { get; set; }

        public int CursorSize
        {
            get => _cursorSize;
            set
            {
                if (_cursorSize == value)
                    return;

                _cursorSize = value;

                OnCursorChanged(_cursorSize, _showCursor);
            }
        }

        public bool ShowCursor
        {
            get => _showCursor;
            set
            {
                if (_showCursor == value)
                    return;

                _showCursor = value;

                OnCursorChanged(_cursorSize, _showCursor);
            }
        }

        public string InputText
        {
            get => _inputText;
            private set
            {
                Require.NotNull(() => value);

                if (_inputText == value)
                    return;

                _inputText = value;

                OnRedraw();
            }
        }

        public ConsoleColour InputColor
        {
            get => _inputColor;
            set
            {
                if (_inputColor == value)
                    return;

                _inputColor = value;

                OnRedraw();
            }
        }

        public string SelectedText { get; set; }
        public ConsoleColour SelectedColor { get; set; }

        public string PlaceHolderText { get; set; }
        public ConsoleColour PlaceHolderColor { get; set; }

        public int CharacterLimit { get; set; }

        public bool HasPrefix { get; set; }
        public char PrefixCharacter { get; set; } = '>';

        public bool ShowOverflowPrefixCharacter { get; set; }
        public ConsoleColour PrefixCharacterColor { get; set; } = ConsoleColour.Text;
        public bool AllowReturn { get; set; }
        public bool AllowSelection { get; set; } = true;

        public int CurrentCaretIndex
        {
            get { return _currentCaretIndex; }
            private set
            {
                if (_currentCaretIndex == value)
                    return;

                _currentCaretIndex = value;

                CursorPoint = ContentBox.Point.WithOffset(CurrentCaretIndex, 0);
            }
        }

        public int CurrentTextIndex { get; private set; }

        BufferPoint CursorPoint
        {
            get => _cursorPoint;
            set
            {
                _cursorPoint = value;

                OnCursorPositionChanged(_cursorPoint);
            }
        }

        public override IEnumerable<WriteObject> GetDrawingData()
        {
            InitializeDrawingData();
            InitializePrefixDrawingData();
            return base.GetDrawingData();
        }

        void InitializePrefixDrawingData()
        {
            if (!HasPrefix)
                return;

            if (Padding.Left <= 0)
                return;

            var showChar = PrefixCharacter;

            if (ShowOverflowPrefixCharacter)
            {
                if (InputText.Length >= _size)
                {
                    showChar = InputText[InputText.Length - _size];
                }
            }

            var write = new WriteObject(showChar.ToString(), ContentBox.Point.WithOffset(-1, 0), PrefixCharacterColor, Elevation);
            ContentWrite.Add(write);
        }

        public void AppendText(string text)
        {
            if (InputText.Length + text.Length < _size)
                CurrentCaretIndex += text.Length;

            CurrentTextIndex += text.Length;
            InputText = InputText.Insert(CurrentTextIndex - text.Length, text);
        }

        protected override void OnFocused()
        {
            base.OnFocused();

            OnCursorChanged(_cursorSize, _showCursor);
        }

        protected override void ProcessKeyPress(ConsoleKeyInfo key)
        {
            base.ProcessKeyPress(key);

            if (!HasFocus)
                return;

            var ch = key.KeyChar;

            if (!char.IsControl(ch))
                AppendText($"{ch}");
            else if (key.Key == ConsoleKey.Backspace)
            {
                RemoveCharacter(CurrentTextIndex);
            }
            else if (key.Key == ConsoleKey.LeftArrow)
            {
                MoveCursorToLeft();
            }
            else if (key.Key == ConsoleKey.RightArrow)
            {
                MoveCursorToRight();
            }
        }

        void RemoveCharacter(int currentTextIndex)
        {
            if (InputText.Length == 0 || CurrentTextIndex == 0)
                return;

            if (InputText.Length < _size)
                CurrentCaretIndex -= 1;

            CurrentTextIndex -= 1;
            InputText = InputText.Remove(currentTextIndex - 1, 1);
        }

        void MoveCursorToLeft()
        {
            if (CurrentTextIndex == 0)
                return;

            if (CurrentCaretIndex > 0)
            {
                CurrentCaretIndex--;
            }

            CurrentTextIndex--;
        }

        void MoveCursorToRight()
        {
            if (CurrentTextIndex + 1 > InputText.Length)
                return;

            if (CurrentCaretIndex < _size - 1)
            {
                CurrentCaretIndex++;
            }

            CurrentTextIndex++;
        }

        void InitializeDrawingData()
        {
            var color = InputColor;

            var startIndex = CurrentTextIndex - CurrentCaretIndex;
            _writtenText = InputText.Substring(startIndex);
            if (InputText.Length >= _size && _writtenText.Length - _size >= 1)
                _writtenText = _writtenText.Remove(_size - 1);

            CursorPoint = ContentBox.Point.WithOffset(CurrentCaretIndex, 0);

            var obj = new WriteObject(_writtenText, ContentBox.Point, color, Elevation);
            ContentWrite.Add(obj);

            IsWritten = true;
        }
    }
}