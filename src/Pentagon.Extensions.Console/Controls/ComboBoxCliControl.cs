// -----------------------------------------------------------------------
//  <copyright file="ComboBoxCliControl.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Resources.Localization;

    public class ComboBoxCliControl<T> : CliControl<IEnumerable<T>>
    {
        static readonly string ErrorHeader = Localization.Get(LocalizationKeyNames.ErrorHeader);
        static readonly string MaxError = Localization.Get(LocalizationKeyNames.MultiSelectMaxError);
        static readonly string MinError = Localization.Get(LocalizationKeyNames.MultiSelectMinError);

        string _error;

        readonly string _decription;

        [NotNull]
        [ItemNotNull]
        readonly List<ComboField<T>> _items = new List<ComboField<T>>();

        Cursor _initialCursor;

        int _currentPointerIndex;
        ConsoleTextTracker _labelTracker;
        ConsoleTextTracker _contentTracker;
        Cursor _contentCursor;
        Cursor _labelEndCursor;
        bool _hasError;
        ConsoleTextTracker _errorTracker;

        public ComboBoxCliControl(string decription, IEnumerable<T> items = null)
        {
            _decription = decription;

            if (items != null)
            {
                _items.AddRange(items.Select(a => new ComboField<T>
                {
                    Content = a
                }));
            }
        }

        public int? Max { get; set; }

        public int? Min { get; set; }

        public CliConsoleColor PointerColor { get; } = new CliConsoleColor(ConsoleColor.DarkCyan);

        public CliConsoleColor ItemTextSelectedColor { get; } = new CliConsoleColor(ConsoleColor.Gray);

        public CliConsoleColor ItemPrefixSelectedColor { get; } = new CliConsoleColor(ConsoleColor.DarkGreen);

        public CliConsoleColor ItemPrefixColor { get; } = new CliConsoleColor(ConsoleColor.Gray);

        public CliConsoleColor ItemTextColor { get; } = new CliConsoleColor(ConsoleColor.Gray);

        public char PointerCharacter { get; } = '>';

        public char SelectCharacter { get; } = '*';

        /// <inheritdoc />
        public override IEnumerable<T> Run()
        {
            ConsoleHelper.EnsureNewLine();
            var cursorShow = Cursor.Current.Show;
            Cursor.SetCurrent(c => c.Show = false);
            _initialCursor = Cursor.Current;

            while (true)
            {
                var initialWidth = Console.WindowWidth;
                Write();

                var i = Console.ReadKey(true);

                if (ProccessInput(i))
                {
                    if (_items.Count(a => a.IsSelected) > Max)
                    {
                        _hasError = true;
                        _error = string.Format(MaxError, Max);
                        continue;
                    }

                    if (_items.Count(a => a.IsSelected) < Min)
                    {
                        _hasError = true;
                        _error = string.Format(MinError, Min);
                        continue;
                    }

                    _hasError = false;

                    break;
                }

                if (initialWidth != Console.WindowWidth)
                {
                    _labelTracker?.ClearAll();
                    _contentTracker?.ClearAll();
                    _errorTracker?.ClearAll();
                }
            }

            _contentTracker?.ClearAll();
            _errorTracker?.ClearAll();

            _labelEndCursor?.Apply();

            ConsoleWriter.Write(_items.Where(a => a.IsSelected).ToList().Select(a => a.Content.ToString()).Aggregate((a, b) => $"{a}, {b}"), ConsoleColor.DarkCyan);
            Console.WriteLine();

            Cursor.SetCurrent(c => c.Show = cursorShow);

            return _items.Where(a => a.IsSelected).Select(a => a.Content);
        }

        public void AddItem(T item)
        {
            _items.Add(new ComboField<T>
            {
                Content = item
            });
        }

        /// <inheritdoc />
        protected override void Write()
        {
            _initialCursor.Apply();

            using (_labelTracker = new ConsoleTextTracker())
            {
                WriteLabel(_decription);
            }

            _labelEndCursor = Cursor.Current.Offset(1, 0);

            ConsoleHelper.EnsureNewLine();

            _contentCursor = Cursor.Current;

            Cursor.Current.Offset(1, 0).Apply();

            using (_contentTracker = new ConsoleTextTracker())
            {
                for (var i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];

                    var off = Cursor.Current.Y;
                    WriteItem(item);
                    item.RowSpan = Cursor.Current.Y - off + 1;

                    if (i < _items.Count - 1)
                        Cursor.Current.Offset(0, 1).Copy(c => c.X = 2).Apply();
                }

                WritePointer(_contentCursor);
            }

            _contentCursor.Offset(0, _items.Select(a => a.RowSpan).Sum()).EnsureNewLine().Apply();

            using (_errorTracker = new ConsoleTextTracker())
            {
                var errorHeader = ErrorHeader;
                var errorLength = errorHeader.Length + Math.Max(MaxError.Length, MinError.Length) + 1;

                if (_hasError)
                {
                    Console.WriteLine();
                    ConsoleWriter.Write(errorHeader, ConsoleColor.Red);
                    Console.Write(value: " ");
                    ConsoleWriter.Write(_error);
                }
                else
                {
                    Console.WriteLine();
                    ConsoleHelper.WriteSpace(errorLength);
                }
            }
        }

        bool ProccessInput(ConsoleKeyInfo input)
        {
            if (input.Key == ConsoleKey.UpArrow)
                _currentPointerIndex = _currentPointerIndex == 0 ? _items.Count - 1 : _currentPointerIndex - 1;
            else if (input.Key == ConsoleKey.DownArrow)
                _currentPointerIndex = _currentPointerIndex == _items.Count - 1 ? 0 : _currentPointerIndex + 1;
            else if (input.Key == ConsoleKey.Spacebar)
                _items[_currentPointerIndex].IsSelected ^= true;
            else if (input.Key == ConsoleKey.Enter)
                return true;

            return false;
        }

        void WritePointer([NotNull] Cursor initial)
        {
            for (var i = 0; i < _items.Count; i++)
                ConsoleWriter.Write(' ', CliContext.ColorScheme.Blank, 1, initial.Y + i + _items.Select(a => a.RowSpan - 1).Where((_, ind) => ind < i).Sum());

            ConsoleWriter.Write(PointerCharacter,
                                PointerColor,
                                1,
                                initial.Y + _currentPointerIndex + _items.Select(a => a.RowSpan - 1).Where((_, i) => i < _currentPointerIndex).Sum());
        }

        void WriteItem([NotNull] ComboField<T> item)
        {
            ConsoleWriter.Write($"({(item.IsSelected ? SelectCharacter.ToString() : " ")})", item.IsSelected ? ItemPrefixSelectedColor : ItemPrefixColor);
            ConsoleHelper.WriteSpace();
            ConsoleWriter.Write(item.Content, item.IsSelected ? ItemTextSelectedColor : ItemTextColor);
        }
    }
}