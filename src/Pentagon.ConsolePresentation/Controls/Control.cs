// -----------------------------------------------------------------------
//  <copyright file="Control.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Borders;
    using Buffers;
    using ColorSystem;
    using Enums;
    using Structures;

    public abstract class Element
    {
        protected IList<WriteObject> ContentWrite = new List<WriteObject>();

        static int _elevation;
        BorderBuilder _borderBuilder;
        IEnumerable<WriteObject> _borderObjects = new WriteObject[] { };
        IEnumerable<WriteObject> _paddingObjects = new WriteObject[] { };
        IEnumerable<WriteObject> _marginObjects = new WriteObject[] { };
        Thickness _padding;
        Border _border;
        BufferPoint _point;
        Thickness _margin;
        
        internal event EventHandler<IEnumerable<WriteObject>> CleanRequested;

        internal event EventHandler PositionChanged;

        internal event EventHandler SizeChanged;

        internal event EventHandler<CursorEventArgs> CursorChanged;

        internal event EventHandler RedrawRequested;

        public int Elevation { get; } = ++_elevation;

        public Box ContentBox => GetContentBox();

        public Box ControlBox => new Box(_point,
                                         ContentWidth + Padding.Horizontal + Border.Thickness.Horizontal + _margin.Horizontal,
                                         ContentHeight + Padding.Vertical + Border.Thickness.Vertical + _margin.Vertical);

        public Box MarginBox
        {
            get
            {
                var box = ControlBox;
                box.ExcludeBox(BorderBox);
                return box;
            }
        }

        public Thickness Padding
        {
            get => _padding;
            set
            {
                if (_padding == value)
                    return;

                _padding = value;

                OnSizeChanged(ContentWidth, ContentHeight);
            }
        }

        public Thickness Margin
        {
            get => _margin;
            set
            {
                if (!IsPositioningEnabled && IsWritten)
                    throw new InvalidOperationException(message: "Positioning is not enabled.");

                _margin = value;

                OnSizeChanged(ContentWidth, ContentHeight);
            }
        }

        public HorizontalAlignment HorizontalContentAlignment { get; set; } = HorizontalAlignment.Center;

        public VerticalAlignment VerticalContentAlignment { get; set; } = VerticalAlignment.Center;

        public Border Border
        {
            get => _border;
            set
            {
                if (_border == value)
                    return;

                _border = value;

                OnSizeChanged(ContentWidth, ContentHeight);
            }
        }

        public BufferPoint Point
        {
            get => _point;
            set
            {
                if (!IsPositioningEnabled && IsWritten)
                    throw new InvalidOperationException(message: "Positioning is not enabled.");

                if (_point == value)
                    return;

                _point = value;

                OnRedraw();
            }
        }

        public int ContentWidth { get; private set; }

        public int ContentHeight { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public bool IsWritten { get; protected set; }

        internal bool IsPositioningEnabled { get; set; }

        Box BorderBox => GetBorderBox();

        Box PaddingBox => GetPaddingBox();

        public virtual Size Measure(Size availableSize)
        {
            var margin = Margin;
            var size = new Size(Math.Max(availableSize.Width - margin.Horizontal,0), Math.Max(availableSize.Height - margin.Vertical, 0));
            return size;
        }

        public virtual IEnumerable<WriteObject> GetDrawingData()
        {
            InitializeBorderDrawData();
            InitializePaddingDrawData();
            return _borderObjects.Concat(_paddingObjects).Concat(_marginObjects).Concat(ContentWrite);
        }

        public void Clear()
        {
            // _borderWriter.Clear();
            CleanRequested?.Invoke(this, ContentWrite.Concat(_borderObjects));
            //foreach (var writeObject in ContentWrite)
            //    Screen.CleanObject(writeObject);

            IsWritten = false;
        }

        protected void OnSizeChanged(int width, int height)
        {
            if (width != 0 && height != 0)
            {
                ContentWidth = width;
                ContentHeight = height;

                Width = Margin.Horizontal + Border.Thickness.Horizontal + Padding.Horizontal + ContentWidth;
                Height = Margin.Vertical + Border.Thickness.Vertical + Padding.Vertical + ContentHeight;

                SizeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected void OnCursorChanged(int size, bool show)
        {
            CursorChanged?.Invoke(this, new CursorEventArgs(size, show));
        }

        protected void OnCursorPositionChanged(BufferPoint newPoint)
        {
            CursorChanged?.Invoke(this, new CursorEventArgs(newPoint));
        }

        protected void OnRedraw()
        {
            RedrawRequested?.Invoke(this, EventArgs.Empty);
            //if (IsWritten)
            //    Draw();
        }

        internal void SetPoint(BufferPoint point)
        {
            _point = point;
        }

        Box GetPaddingBox()
        {
            var width = ControlBox.Width - _margin.Horizontal - Border.Thickness.Horizontal;
            var height = ControlBox.Height - _margin.Vertical - Border.Thickness.Vertical;

            var xcoord = ControlBox.Point.X + _margin.Left + Border.Thickness.Left;
            var ycoord = ControlBox.Point.Y + _margin.Top + Border.Thickness.Top;

            var box = new Box(new BufferPoint(xcoord, ycoord), width, height);
            box.ExcludeBox(ContentBox);
            return box;
        }

        Box GetBorderBox()
        {
            var width = ControlBox.Width - _margin.Horizontal;
            var height = ControlBox.Height - _margin.Vertical;

            var xcoord = ControlBox.Point.X + _margin.Left;
            var ycoord = ControlBox.Point.Y + _margin.Top;

            return new Box(new BufferPoint(xcoord, ycoord), width, height);
        }

        Thickness GetRealPadding()
        {
            var p = Padding;

            switch (HorizontalContentAlignment)
            {
                case HorizontalAlignment.Left:
                    p.Right += p.Left;
                    p.Left = 0;
                    break;

                case HorizontalAlignment.Right:
                    p.Left += p.Right;
                    p.Right = 0;
                    break;
            }

            switch (VerticalContentAlignment)
            {
                case VerticalAlignment.Top:
                    p.Bottom += p.Top;
                    p.Top = 0;
                    break;

                case VerticalAlignment.Bottom:
                    p.Top += p.Bottom;
                    p.Bottom = 0;
                    break;
            }

            return p;
        }

        Box GetContentBox()
        {
            var padding = GetRealPadding();
            var width = ControlBox.Width - _margin.Horizontal - Border.Thickness.Horizontal - padding.Horizontal;
            var height = ControlBox.Height - _margin.Vertical - Border.Thickness.Vertical - padding.Vertical;

            var xcoord = ControlBox.Point.X + _margin.Left + Border.Thickness.Left + padding.Left;
            var ycoord = ControlBox.Point.Y + _margin.Top + Border.Thickness.Top + padding.Top;

            return new Box(new BufferPoint(xcoord, ycoord), width, height);
        }

        void InitializeBorderDrawData()
        {
            if (Border.Thickness == Thickness.Zero)
                return;

            _borderBuilder = new BorderBuilder(Border, PaddingBox, Elevation);
            _borderObjects = _borderBuilder.GetDrawingData();
        }

        void InitializePaddingDrawData()
        {
            if (Padding != Thickness.Zero)
                _paddingObjects = PaddingBox.GetRowBoxes().Select(b => new WriteObject(b, new ConsoleColour(DefaultColorScheme.Blue), Elevation, '~'));
            if (Margin != Thickness.Zero)
                _marginObjects = MarginBox.GetRowBoxes().Select(b => new WriteObject(b, new ConsoleColour(DefaultColorScheme.Red), Elevation, '~'));
        }
    }

    public abstract class Control : Element
    {
        bool _hasFocus;

        public bool CanFocus { get; protected set; }

        public bool HasFocus
        {
            get => _hasFocus;
            internal set
            {
                _hasFocus = value;

                if (_hasFocus)
                    OnFocused();
            }
        }

        public Control()
        {
            InputListener.Current.KeyPressed += (s, e) => { ProcessKeyPress(e); };
        }

        protected virtual void ProcessKeyPress(ConsoleKeyInfo key) { }
        
        protected virtual void OnFocused() { }
    }
}