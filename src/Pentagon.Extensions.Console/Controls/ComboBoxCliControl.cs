namespace Pentagon.Extensions.Console.Controls {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    public class ComboBoxCliControl : CliControl<IEnumerable<ComboField>>
    {
        readonly string _decription;
        Cursor _initialCursor;

        [NotNull]
        [ItemNotNull]
        List<ComboField> _items = new List<ComboField>();

        CliConsoleColor _itemTextSelectedColor = new CliConsoleColor(ConsoleColor.Gray);
        CliConsoleColor _itemPrefixSelectedColor = new CliConsoleColor(ConsoleColor.DarkGreen);
        CliConsoleColor _itemPrefixColor = new CliConsoleColor(ConsoleColor.Gray); 
        CliConsoleColor _itemTextColor = new CliConsoleColor(ConsoleColor.Gray);

        public ComboBoxCliControl(string decription, IEnumerable<string> items = null)
        {
            _decription = decription;

            if (items != null)
                _items.AddRange(items.Select(a=> new ComboField
                                                 {
                                                         Text = a ?? string.Empty
                                                 }));
        }

        public void AddItem(string item)
        {
            _items.Add(new ComboField
                       {
                               Text = item ?? string.Empty
                       });
        }

        /// <inheritdoc />
        public override IEnumerable<ComboField> Run()
        {
            ConsoleHelper.EnsureNewLine();
            Cursor.SetCurrent(c => c.Show =true);
            _initialCursor = Cursor.Current;

            while (true)
            {
                Write();

                var i = Console.ReadKey(true);

                if (ProccessInput(i))
                {
                    break;
                }
            }

            return _items;
        }

        bool ProccessInput(ConsoleKeyInfo input)
        {
            if (input.Key == ConsoleKey.UpArrow)
            {
                _currentPointerIndex = _currentPointerIndex == 0 ? _items.Count-1 : _currentPointerIndex - 1;
            }
            else if (input.Key == ConsoleKey.DownArrow)
            {
                _currentPointerIndex = _currentPointerIndex == _items.Count - 1 ? 0 : _currentPointerIndex + 1;
            }
            else if (input.Key == ConsoleKey.Spacebar)
            {
                _items[_currentPointerIndex].IsSelected ^= true;
            }
            else if (input.Key == ConsoleKey.Enter)
            {
                return true;
            }

            return false;
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

                if (i < _items.Count -1 )
                    Cursor.Current.Offset(0, 1).Configure(c => c.X = 2).Apply();
            }

            WritePointer(readPosition);

            readPosition.Offset(0, _items.Select(a => a.RowSpan).Sum()).EnsureNewLine().Apply();
        }

        int _currentPointerIndex;
        char _pointerCharacter = '>';
        CliConsoleColor _pointerColor = new CliConsoleColor(ConsoleColor.DarkCyan);

        void WritePointer([NotNull] Cursor initial)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                ConsoleWriter.Write(' ', Cli.ColorScheme.Blank, 1, initial.Y  + i + _items.Select(a => a.RowSpan - 1).Where((_, ind) => ind < i).Sum());
            }

            ConsoleWriter.Write(_pointerCharacter,
                                _pointerColor,
                                1,
                                initial.Y + _currentPointerIndex + _items.Select(a => a.RowSpan - 1).Where((_,i) => i < _currentPointerIndex).Sum());
        }

        void WriteItem([NotNull] ComboField item)
        {
            ConsoleHelper.Write($"({(item.IsSelected ? "*" : " ")})", item.IsSelected ? _itemPrefixSelectedColor : _itemPrefixColor);
            ConsoleHelper.WriteSpace();
            ConsoleHelper.Write(item.Text, item.IsSelected ? _itemTextSelectedColor : _itemTextColor);
        }
    }
}