// -----------------------------------------------------------------------
//  <copyright file="Screen.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Buffers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ColorSystem;
    using Controls;
    using Structures;

    public class Screen<TTheme> : IScreen
    {
        readonly IColorManager _colorManager;
        readonly IScreenCellCache _cellCache;
        readonly IConsoleCleaner _cleaner;
        readonly IConsoleWriter _writer;
        bool _isActive;
        int _width;
        int _height;
        string _title;

        public Screen(IConsoleWriter writer, IConsoleCleaner cleaner, IColorManager colorManager, IScreenCellCache cellCache)
        {
            _colorManager = colorManager;
            _cellCache = cellCache;
            _cleaner = cleaner;
            _writer = writer;
            Cursor = new Cursor(this);
            Cursor.Show = false;
        }

        /// <inheritdoc />
        public Cursor Cursor { get; }

        /// <inheritdoc />
        public int Width
        {
            get => _width;
            set
            {
                _width = value;

                if (IsActive)
                    Console.BufferWidth = _width;
            }
        }

        /// <inheritdoc />
        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                if (IsActive)
                    Console.BufferHeight = _height;
            }
        }

        /// <inheritdoc />
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;

                if (IsActive)
                    Console.Title = _title;
            }
        }

        /// <inheritdoc />
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value)
                    return;

                _isActive = value;

                Cursor.Refresh();
            }
        }

        /// <inheritdoc />
        public IList<WriteObject> Objects { get; private set; } = new List<WriteObject>();

        public IDictionary<BufferPoint, SortedList<int, BufferCell>> CellCache => _cellCache.Cache;

        /// <inheritdoc />
        public Panel Panel { get; private set; }

        public Type ColorTheme => typeof(TTheme);

        public IScreen WithPanel(Panel panel)
        {
            if (Panel != null)
                return this;

            Panel = panel;

            Panel.CursorChanged += (s, e) =>
                                   {
                                       if (e.Point.HasValue)
                                           Cursor.Coord = e.Point.Value;
                                       if (e.Show.HasValue)
                                           Cursor.Show = e.Show.Value;
                                       if (e.Size.HasValue)
                                           Cursor.Size = e.Size.Value;
                                   };

            Panel.ControlRedrawRequested += (s, e) => { DrawControl(e); };

            Panel.ControlCleanupRequested += (s, e) =>
                                             {
                                                 foreach (var writeObject in e)
                                                     CleanObject(writeObject);
                                             };

            return this;
        }

        /// <inheritdoc />
        public bool CanContain(Box box)
        {
            if (!box.IsValid)
                return false;

            return box.Point.X + box.Width <= Width + 1 && box.Point.Y + box.Height <= Height + 1;
        }

        /// <inheritdoc />
        public bool CanContain(BufferPoint point)
        {
            if (!point.IsValid)
                return false;
            // TODO test that
            return point.X <= Width + 1 && point.Y <= Height + 1;
        }

        /// <inheritdoc />
        public void WriteObject(WriteObject writeObject)
        {
            if (!Objects.Any(a => a == writeObject))
                Objects.Add(writeObject);

            if (_writer.Write(this, writeObject))
            {
                var chs = writeObject.Characters.Select(a => new BufferCell(a.character, a.point, writeObject.Color, writeObject.Elevation));
                foreach (var bufferCell in chs)
                    _cellCache.AddOrReplaceCell(bufferCell);
            }
        }

        /// <inheritdoc />
        public void CleanObject(WriteObject writeObject)
        {
            if (!Objects.Any(a => a == writeObject))
                return;

            if (!writeObject.CanRemove)
                return;

            if (_cleaner.Remove(this, writeObject))
            {
                var chs = writeObject.Characters.Select(a => new BufferCell(a.character, a.point, writeObject.Color, writeObject.Elevation));
                foreach (var bufferCell in chs)
                    _cellCache.RemoveCell(bufferCell.Point, bufferCell.Elevation);
            }
        }

        public void DrawControl(Control control)
        {
            if (!Panel.Children.Contains(control))
                Panel.AddControl(control);

            if (control.IsWritten)
                control.Clear();

            var write = control.GetDrawingData();

            foreach (var writeObject in write)
                WriteObject(writeObject);
        }

        public void FocusControl(Control control)
        {
            if (!Panel.Children.Contains(control))
                throw new InvalidOperationException(message: "Given control don't exists on screen.");

            if (!control.IsWritten || !control.CanFocus)
                throw new InvalidOperationException(message: "Control doesn't support focusing.");

            foreach (var c in Panel.Children.Where(a => a.HasFocus))
                c.HasFocus = false;

            control.HasFocus = true;
        }

        public void ApplyColorTheme()
        {
            _colorManager.SetTheme<TTheme>();
        }

        public void Activate()
        {
            if (!IsActive)
                return;

            Title = Title;
            Width = Width;
            Height = Height;
            Cursor.Refresh();
        }

        async Task StartObjectsManagingAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                Objects = Objects.Where(o => o.Status != WriteStatus.Removed).ToList();
            }
        }
    }
}