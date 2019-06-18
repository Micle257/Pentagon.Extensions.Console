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

    public class ComboBoxCliControl<T> : CliControl<IEnumerable<ComboField<T>>>
    {
        readonly string _decription;

        [NotNull]
        [ItemNotNull]
        readonly List<ComboField<T>> _items = new List<ComboField<T>>();

        Cursor _initialCursor;

        int _currentPointerIndex;

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

        public CliConsoleColor PointerColor { get; } = new CliConsoleColor(ConsoleColor.DarkCyan);

        public CliConsoleColor ItemTextSelectedColor { get; } = new CliConsoleColor(ConsoleColor.Gray);
        public CliConsoleColor ItemPrefixSelectedColor { get; } = new CliConsoleColor(ConsoleColor.DarkGreen);
        public CliConsoleColor ItemPrefixColor { get; } = new CliConsoleColor(ConsoleColor.Gray);
        public CliConsoleColor ItemTextColor { get; } = new CliConsoleColor(ConsoleColor.Gray);
        public char PointerCharacter { get; } = '>';
        public char SelectCharacter { get; } = '*';

        /// <inheritdoc />
        public override IEnumerable<ComboField<T>> Run()
        {
            ConsoleHelper.EnsureNewLine();
            var cursorShow = Cursor.Current.Show;
            Cursor.SetCurrent(c => c.Show = false);
            _initialCursor = Cursor.Current;

            while (true)
            {
                Write();

                var i = Console.ReadKey(true);

                if (ProccessInput(i))
                    break;
            }

            Cursor.SetCurrent(c => c.Show = cursorShow);

            return _items;
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

            WriteLabel(_decription);

            Console.WriteLine();

            var readPosition = Cursor.Current;

            Cursor.Current.Offset(1, 0).Apply();

            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];

                var off = Cursor.Current.Y;
                WriteItem(item);
                item.RowSpan = Cursor.Current.Y - off + 1;

                if (i < _items.Count - 1)
                    Cursor.Current.Offset(0, 1).Configure(c => c.X = 2).Apply();
            }

            WritePointer(readPosition);

            readPosition.Offset(0, _items.Select(a => a.RowSpan).Sum()).EnsureNewLine().Apply();
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
                ConsoleWriter.Write(' ', Cli.ColorScheme.Blank, 1, initial.Y + i + _items.Select(a => a.RowSpan - 1).Where((_, ind) => ind < i).Sum());

            ConsoleWriter.Write(PointerCharacter,
                                PointerColor,
                                1,
                                initial.Y + _currentPointerIndex + _items.Select(a => a.RowSpan - 1).Where((_, i) => i < _currentPointerIndex).Sum());
        }

        void WriteItem([NotNull] ComboField<T> item)
        {
            ConsoleHelper.Write($"({(item.IsSelected ? SelectCharacter.ToString() : " ")})", item.IsSelected ? ItemPrefixSelectedColor : ItemPrefixColor);
            ConsoleHelper.WriteSpace();
            ConsoleHelper.Write(item.Content, item.IsSelected ? ItemTextSelectedColor : ItemTextColor);
        }
    }
}