// -----------------------------------------------------------------------
//  <copyright file="BorderBuilder.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Controls.Borders
{
    using System;
    using System.Collections.Generic;
    using Ascii;
    using Buffers;
    using Data.Helpers;
    using Properties;
    using Structures;

    public class BorderBuilder
    {
        readonly int _elevation;
        readonly bool _hasLeft;
        readonly bool _hasTop;
        readonly bool _hasRight;
        readonly bool _hasBottom;
        readonly IList<WriteObject> _write = new List<WriteObject>();

        IDictionary<(BorderLineType line, BorderLineSegmentType segment), char?> _characterMap;

        public BorderBuilder(Border border, Box insideBox, int elevation)
        {
            _elevation = elevation;
            Border = border;
            InsideBox = insideBox;

            _hasRight = Border.Thickness.Right != 0;
            _hasLeft = Border.Thickness.Left != 0;
            _hasTop = Border.Thickness.Top != 0;
            _hasBottom = Border.Thickness.Bottom != 0;
        }

        public Border Border { get; }
        public Box InsideBox { get; }

        public Box BorderBox => GetBorderBox();

        public IEnumerable<WriteObject> GetDrawingData()
        {
            CalculateDrawData();
            return _write;
        }

        Box GetBorderBox()
        {
            var width = InsideBox.Width + Border.Thickness.Horizontal;
            var height = InsideBox.Height + Border.Thickness.Vertical;

            var xcoord = InsideBox.Point.X - Border.Thickness.Left;
            var ycoord = InsideBox.Point.Y - Border.Thickness.Top;

            return new Box(new BufferPoint(xcoord, ycoord), width, height);
        }

        void CalculateDrawData()
        {
            EnsureInicializeMap();

            if (_hasTop)
                WriteTopBorder();

            if (_hasBottom)
                WriteBottomLine();

            if (_hasLeft)
                WriteLeftLine();

            if (_hasRight)
                WriteRightLine();

            if (_hasTop && _hasLeft)
                WriteTopLeft();
            if (_hasTop && _hasRight)
                WriteTopRight();
            if (_hasBottom && _hasLeft)
                WriteBottomLeft();
            if (_hasBottom && _hasRight)
                WriteBottomRight();
        }

        void WriteBottomRight()
        {
            var obj = new WriteObject(GetSegment(Border.BottomLine.Type, BorderLineSegmentType.BottomRightCorner).ToString(),
                                      new BufferPoint(BorderBox.Point.X + InsideBox.Width + (_hasLeft ? 1 : 0), BorderBox.Point.Y + InsideBox.Height + (_hasTop ? 1 : 0)),
                                      Border.BottomLine.Color,
                                      _elevation);
            _write.Add(obj);
        }

        void WriteBottomLeft()
        {
            var obj = new WriteObject(GetSegment(Border.LeftLine.Type, BorderLineSegmentType.BottomLeftCorner).ToString(),
                                      new BufferPoint(BorderBox.Point.X, BorderBox.Point.Y + InsideBox.Height + (_hasTop ? 1 : 0)),
                                      Border.BottomLine.Color,
                                      _elevation);
            _write.Add(obj);
        }

        void WriteTopRight()
        {
            var obj = new WriteObject(GetSegment(Border.RightLine.Type, BorderLineSegmentType.TopRightCorner).ToString(),
                                      new BufferPoint(BorderBox.Point.X + InsideBox.Width + (_hasLeft ? 1 : 0), BorderBox.Point.Y),
                                      Border.TopLine.Color,
                                      _elevation);
            _write.Add(obj);
        }

        void WriteTopLeft()
        {
            var obj = new WriteObject(GetSegment(Border.TopLine.Type, BorderLineSegmentType.TopLeftCorner).ToString(),
                                      new BufferPoint(BorderBox.Point.X, BorderBox.Point.Y),
                                      Border.TopLine.Color,
                                      _elevation);
            _write.Add(obj);
        }

        void WriteRightLine()
        {
            var obj = new WriteObject(new string(GetSegment(Border.RightLine.Type, BorderLineSegmentType.Vertical), InsideBox.Height),
                                      new BufferPoint(BorderBox.Point.X + InsideBox.Width + (_hasLeft ? 1 : 0), BorderBox.Point.Y + (_hasTop ? 1 : 0)),
                                      Border.RightLine.Color,
                                      _elevation,
                                      true);
            _write.Add(obj);
        }

        void WriteLeftLine()
        {
            var obj = new WriteObject(new string(GetSegment(Border.LeftLine.Type, BorderLineSegmentType.Vertical), InsideBox.Height),
                                      new BufferPoint(BorderBox.Point.X, BorderBox.Point.Y + (_hasTop ? 1 : 0)),
                                      Border.LeftLine.Color,
                                      _elevation,
                                      true);
            _write.Add(obj);
        }

        void WriteBottomLine()
        {
            var obj = new WriteObject(new string(GetSegment(Border.BottomLine.Type, BorderLineSegmentType.Horizontal), InsideBox.Width),
                                      new BufferPoint(BorderBox.Point.X + (_hasLeft ? 1 : 0), BorderBox.Point.Y + InsideBox.Height + (_hasTop ? 1 : 0)),
                                      Border.BottomLine.Color,
                                      _elevation);
            _write.Add(obj);
        }

        void WriteTopBorder()
        {
            var obj = new WriteObject(new string(GetSegment(Border.TopLine.Type, BorderLineSegmentType.Horizontal), InsideBox.Width),
                                      new BufferPoint(BorderBox.Point.X + (_hasLeft ? 1 : 0), BorderBox.Point.Y),
                                      Border.TopLine.Color,
                                      _elevation);
            _write.Add(obj);
        }

        char GetSegment(BorderLineType lineType, BorderLineSegmentType segmentType)
        {
            EnsureInicializeMap();

            if (_characterMap.TryGetValue((lineType, BorderLineSegmentType.Any), out var ch))
            {
                if (!ch.HasValue)
                    throw new ArgumentException(message: "There is no character for the given border segment."); // TODO better
                return ch.Value;
            }
            if (_characterMap.TryGetValue((lineType, segmentType), out ch))
            {
                if (!ch.HasValue)
                    throw new ArgumentException(message: "There is no character for the given border segment."); // TODO better
                return ch.Value;
            }

            throw new ArgumentOutOfRangeException(paramName: "There is no value for the given border segment.");
        }

        void EnsureInicializeMap()
        {
            if (_characterMap != null)
                return;

            Ascii.Initialize();

            _characterMap = new Dictionary<(BorderLineType line, BorderLineSegmentType segment), char?>
                            {
                                {
                                    (BorderLineType.Line, BorderLineSegmentType.Horizontal),
                                    Ascii.Extended.BoxSingleHorizontalLine?.Char
                                },
                                {
                                    (BorderLineType.Line, BorderLineSegmentType.Vertical),
                                    Ascii.Extended.BoxSingleVerticalLine?.Char
                                },
                                {
                                    (BorderLineType.Line, BorderLineSegmentType.TopLeftCorner),
                                    Ascii.Extended.BoxSingleLineTopLeftCorner?.Char
                                },
                                {
                                    (BorderLineType.Line, BorderLineSegmentType.TopRightCorner),
                                    Ascii.Extended.BoxSingleLineTopRightCorner?.Char
                                },
                                {
                                    (BorderLineType.Line, BorderLineSegmentType.BottomLeftCorner),
                                    Ascii.Extended.BoxSingleLineBottomLeftCorner?.Char
                                },
                                {
                                    (BorderLineType.Line, BorderLineSegmentType.BottomRightCorner),
                                    Ascii.Extended.BoxSingleLineBottomRightCorner?.Char
                                },

                                {
                                    (BorderLineType.DoubleLine, BorderLineSegmentType.Horizontal),
                                    Ascii.Extended.BoxDoubleHorizontalLine?.Char
                                },
                                {
                                    (BorderLineType.DoubleLine, BorderLineSegmentType.Vertical),
                                    Ascii.Extended.BoxDoubleVerticalLine?.Char
                                },
                                {
                                    (BorderLineType.DoubleLine, BorderLineSegmentType.TopLeftCorner),
                                    Ascii.Extended.BoxDoubleLineTopLeftCorner?.Char
                                },
                                {
                                    (BorderLineType.DoubleLine, BorderLineSegmentType.TopRightCorner),
                                    Ascii.Extended.BoxDoubleLineTopRightCorner?.Char
                                },
                                {
                                    (BorderLineType.DoubleLine, BorderLineSegmentType.BottomLeftCorner),
                                    Ascii.Extended.BoxDoubleLineBottomLeftCorner?.Char
                                },
                                {
                                    (BorderLineType.DoubleLine, BorderLineSegmentType.BottomRightCorner),
                                    Ascii.Extended.BoxDoubleLineBottomRightCorner?.Char
                                },

                                {
                                    (BorderLineType.Box, BorderLineSegmentType.Any),
                                    ResourceReader.ReadJson<AsciiCode>(Resources.Block)?.Char
                                }
                            };
        }
    }
}