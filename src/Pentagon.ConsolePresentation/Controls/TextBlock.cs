// -----------------------------------------------------------------------
//  <copyright file="TextBlock.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Buffers;
    using Enums;
    using JetBrains.Annotations;
    using Structures;

    public class TextBlock : Control
    {
        ConsoleColour _color = ConsoleColour.Text;

        [NotNull]
        object _data = "";

        readonly int _textStartIndex = 0;
        int _contentWidth;
        int _contentHeight = 1;
        TextTrimming _textTrimming = TextTrimming.Ellipsis;
        TextWrapping _textWrapping = TextWrapping.NoWrap;
        TextAlignment _textAlignment = TextAlignment.Left;

        public TextBlock()
        {
            Size = 10;
        }

        public TextAlignment TextAlignment
        {
            get => _textAlignment;
            set
            {
                if (_textAlignment == value)
                    return;

                _textAlignment = value;

                OnRedraw();
            }
        }

        public int Size
        {
            get => _contentWidth;
            set
            {
                if (_contentWidth == value)
                    return;

                _contentWidth = value;

                switch (TextWrapping)
                {
                    case TextWrapping.NoWrap:
                        _contentHeight = 1;
                        break;
                    case TextWrapping.Wrap:
                        _contentHeight = GetWrappedText().Count();
                        break;
                }

                OnSizeChanged(_contentWidth, _contentHeight);
            }
        }

        [NotNull]
        public object Data
        {
            get => _data;
            set
            {
                Require.NotNull(() => value);

                if (_data.ToString() == value.ToString())
                    return;

                _data = value;

                OnRedraw();
            }
        }

        public ConsoleColour Color
        {
            get => _color;
            set
            {
                if (_color == value)
                    return;

                _color = value;

                OnRedraw();
            }
        }

        public TextTrimming TextTrimming
        {
            get => _textTrimming;
            set
            {
                if (_textTrimming == value)
                    return;

                _textTrimming = value;

                OnRedraw();
            }
        }

        public TextWrapping TextWrapping
        {
            get => _textWrapping;
            set
            {
                if (_textWrapping == value)
                    return;

                _textWrapping = value;

                _contentHeight = GetWrappedText().Count();
                OnSizeChanged(_contentWidth, _contentHeight);
            }
        }

        public override IEnumerable<WriteObject> GetDrawingData()
        {
            InitializeDrawingData();
            return base.GetDrawingData();
        }

        void InitializeDrawingData()
        {
            var text = Data as string ?? Data.ToString();

            var color = Color;

            // overflow handling
            //if (coord.X + text.Length - 1 > _window.BufferWidth)
            //{
            //    coord = _window.Cursor.Coord;
            //    color = _overflowColor;
            //}

            var coord = ContentBox.Point;

            // fit the text into size
            if (_textWrapping == TextWrapping.NoWrap)
            {
                if (text.Length > _contentWidth)
                {
                    text = text.Remove(_contentWidth - 1);
                    if (_contentWidth >= 3 && _textTrimming == TextTrimming.Ellipsis)
                        text = text.Remove(_contentWidth - 4) + "...";
                }

                if (_textAlignment == TextAlignment.Right)
                {
                    var indexOffset = _contentWidth - text.Length;
                    text = $"{new string(' ', indexOffset)}{text}";
                }
                else if (_textAlignment == TextAlignment.Center && text.Length < _contentWidth - 1)
                {
                    var indexOffset = (int) (_contentWidth / 2d) - text.Length + (text.Length % 2 == 1 ? 1 : 2) + (int) ((text.Length - 1) / 2d);
                    text = $"{new string(' ', indexOffset)}{text}";
                }

                var obj = new WriteObject(text, coord.WithOffset(_textStartIndex, 0), color, Elevation);
                ContentWrite.Add(obj);
            }
            else if (_textWrapping == TextWrapping.Wrap)
            {
                var lines = GetWrappedText();
                var yOffset = 0;
                foreach (var line in lines)
                {
                    var lineText = line;
                    if (_textAlignment == TextAlignment.Right)
                    {
                        var indexOffset = _contentWidth - lineText.Length;
                        lineText = $"{new string(' ', indexOffset)}{lineText}";
                    }
                    else if (_textAlignment == TextAlignment.Center && lineText.Length < _contentWidth - 1)
                    {
                        var indexOffset = (int) (_contentWidth / 2d) - lineText.Length + (lineText.Length % 2 == 1 ? 1 : 2) + (int) ((lineText.Length - 1) / 2d);
                        lineText = $"{new string(' ', indexOffset)}{lineText}";
                    }

                    var obj = new WriteObject(lineText, coord.WithOffset(0, yOffset), color, Elevation);
                    ContentWrite.Add(obj);
                    yOffset++;
                }
            }

            IsWritten = true;
        }

        IEnumerable<string> GetWrappedText()
        {
            var text = _data as string ?? _data.ToString();
            var textLength = text.Length;
            var boxSize = _contentWidth;

            var lineCount = Math.Ceiling(textLength / (double) boxSize);

            for (var i = 0; i < lineCount; i++)
            {
                if (i == lineCount - 1)
                    yield return text.Substring(i * _contentWidth);
                else
                    yield return text.Substring(i * _contentWidth, _contentWidth);
            }
        }
    }
}