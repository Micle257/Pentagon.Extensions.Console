// -----------------------------------------------------------------------
//  <copyright file="Menu.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Controls.Menu
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Buffers;
    using EventArguments;
    using Inputs;
    using Pentagon.Extensions;
    using Structures;

    /// <summary> Represents an console's selectable menu. </summary>
    public class Menu : ISelectable<MenuItem>
    {
        public MenuExitStatus CurrentExitStatus;
        //public InputType InputType { get; set; }

        internal bool inputSelected;

        readonly List<MenuItem> _objects = new List<MenuItem>();
        readonly ConsoleKey nextKey = ConsoleKey.DownArrow;
        readonly ConsoleKey prevKey = ConsoleKey.UpArrow;
        readonly ConsoleKey finishKey = ConsoleKey.LeftArrow;
        readonly ConsoleKey pressKey = ConsoleKey.RightArrow;

        /// <summary> Initializes a new instance of the <see cref="Menu" /> class. </summary>
        /// <param name="type"> The type. </param>
        /// <param name="menuItems"> The menu items. </param>
        /// <param name="xPos"> The x position. </param>
        /// <param name="yPos"> The y position. </param>
        public Menu(MenuType type, IEnumerable<string> menuItems, int xPos, int yPos) : this(type, xPos, yPos)
        {
            foreach (var item in menuItems)
            {
                var obj = new MenuItem(this, item);
            }
        }

        /// <summary> Initializes a new instance of the <see cref="Menu" /> class. </summary>
        /// <param name="type"> The type. </param>
        /// <param name="xPos"> The x position. </param>
        /// <param name="yPos"> The y position. </param>
        public Menu(MenuType type, int xPos, int yPos)
        {
            Type = type;
            Objects = new List<MenuItem>();
            Coord = new BufferPoint(xPos, yPos);
            ClearAfter = false;
            SelectingChar = ' ';
            IsActive = true;
            IsSubMenu = false;

            switch (type)
            {
                case MenuType.VerticalInput:
                case MenuType.Vertical:
                    SelectingChar = '>';
                    break;

                case MenuType.Horizontal:
                    nextKey = ConsoleKey.RightArrow;
                    prevKey = ConsoleKey.LeftArrow;
                    finishKey = ConsoleKey.UpArrow;
                    pressKey = ConsoleKey.Enter;
                    break;
            }
        }

        /// <summary> Occurs when this <see cref="Menu" /> finishes loop proccesure. </summary>
        public event EventHandler<MenuExitStatus> Finished;

        public event SelectedEventHandler<MenuItem> Selected;
        internal event EventHandler LoopStarting;

        /// <summary> Gets the type of this <see cref="Menu" />. </summary>
        public MenuType Type { get; }

        /// <summary> Gets collection of <see cref="MenuItem" /> in this container. </summary>
        public List<MenuItem> Objects { get; }

        public List<string> Values
        {
            get { return Objects.Where(a => !a.IsController).Select(a => a.Value).ToList(); }
        }

        /// <summary> Gets the starting position coord of this <see cref="Menu" />. </summary>
        public BufferPoint Coord { get; }

        /// <summary> Gets the selecting character of selected menu. </summary>
        public char SelectingChar { get; }

        /// <summary> Gets or sets a value indicating whether menu should clear its data on <see cref="Finished" />. </summary>
        public bool ClearAfter { get; set; }

        /// <summary> Gets a value indicating whether this menu loop is active. </summary>
        public bool IsActive { get; set; }

        /// <summary> Gets a value indicating whether this instance is a sub menu. </summary>
        public bool IsSubMenu { get; internal set; }

        public int Count => _objects?.Count ?? 0;

        public MenuItem Current { get; private set; }

        protected static ConsoleWindow Window => ConsoleWindow.CurrentWindow;

        public MenuItem this[int index] => _objects?[index];

        public int this[MenuItem value] => _objects?.IndexOf(value) ?? -1;

        public void AddItem(MenuItem item) => _objects.Add(item);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<MenuItem> GetEnumerator() => _objects?.GetEnumerator();

        public void Select(MenuItem obj)
        {
            Current?.UnSelect();
            Current = obj;
            if (Current != null)
                Selected?.Invoke(this, new SelectedEventArgs<MenuItem>(Current, Current.Index));
            Current.Select();
        }

        public void SelectNext()
        {
            var nextId = 0;
            do
            {
                nextId = (Current.Index + 1).Mod(Objects.Count);
                Select(Objects.ElementAt(nextId));
            } while (Objects[nextId].IsDisabled);
        }

        public void SelectPrevious()
        {
            var nextId = 0;
            do
            {
                nextId = (Current.Index - 1).Mod(Objects.Count);
                Select(Objects.ElementAt(nextId));
            } while (Objects[nextId].IsDisabled);
        }

        /// <summary> Starts the menu in non-constructed way. </summary>
        /// <param name="type"> The type. </param>
        /// <param name="menuItems"> The menu items. </param>
        /// <param name="xPos"> The x position. </param>
        /// <param name="yPos"> The y position. </param>
        /// <returns> Created menu instance. </returns>
        public static Menu Start(MenuType type, IEnumerable<string> menuItems, int xPos, int yPos)
        {
            var menu = new Menu(type, menuItems, xPos, yPos);
            menu.Run();
            return menu;
        }

        /// <summary> Starts this instance's loop proccesure. </summary>
        /// <exception cref="System.ArgumentOutOfRangeException"> All menu items are disabled. </exception>
        public virtual Menu Run()
        {
            if (Type == MenuType.VerticalInput && Objects.All(a => !a.IsController))
                AddController(str: "Confirm");

            if (IsSubMenu)
                Select(Objects[0]); // if this is submenu first item will be selected

            while (Current.IsDisabled)
            {
                if (Objects.All(a => a.IsDisabled))
                    throw new ArgumentOutOfRangeException(nameof(Objects), message: "All menu items are disabled.");
                SelectNext();
            }

            LoopStarting?.Invoke(this, null);
            MenuLoop();
            foreach (var item in Objects)
            {
                if (item.Value == "" && !item.DefaultValue.IsAnyEqual("", "-"))
                    item.Value = item.DefaultValue;
            }

            Finished?.Invoke(this, CurrentExitStatus);
            return this;
        }

        /// <summary> Adds new <see cref="MenuItem" /> object. </summary>
        /// <param name="str"> The name. </param>
        /// <returns> </returns>
        public MenuItem Add(string str, InputType input)
        {
            var item = new MenuItem(this, str, input);
            return item;
        }

        public MenuItem Add(string str)
        {
            if (Type == MenuType.VerticalInput)
                return Add(str, InputType.String);
            return Add(str, InputType.None);
        }

        public MenuItem AddController(string str)
        {
            var item = Add(str, InputType.None);
            item.NameText.Data = $"[{str}]";
            item.NameText.Coord = new BufferPoint(Coord.X, item.NameText.Y);
            item.IsController = true;
            item.selectChar = new Text(null, ConsoleColour.Text, item.NameText.X, item.NameText.Y);
            return item;
        }

        /// <summary> Sets the menu selected position. </summary>
        /// <param name="pos"> The position. </param>
        public void SetMenuPosition(int pos) => Select(Objects[pos.Mod(Objects.Count)]);

        public void Clear()
        {
            foreach (var item in Objects)
            {
                item.TextClear();
                item.SubMenu.Clear();
            }
        }

        public MenuItem FindMenuItemByName(string name)
        {
            foreach (var item in Objects)
            {
                if (item.Name == name)
                    return item;
            }
            return null;
        }

        internal void Print()
        {
            foreach (var item in Objects)
                item.TextPrint();
        }

        void MenuLoop()
        {
            while (IsActive)
            {
                foreach (var item in Objects)
                    item.TextPrint();

                var key = Input.WaitKey().Key;

                if (Type == MenuType.VerticalInput && inputSelected && !Current.IsController)
                {
                    if (key == ConsoleKey.Escape)
                        inputSelected = false;
                    else if (key == nextKey || key == ConsoleKey.Enter)
                        SelectNext();
                    else if (key == prevKey)
                        SelectPrevious();
                    else
                    {
                        // TODO  new InputSimulator().Keyboard.KeyPress((VirtualKeyCode) key);
                        Current.Input.RunOnce();
                        Current.Value = Current.Input.InputText;
                    }
                    inputSelected &= !Current.IsController;

                    continue;
                }

                if (key == ConsoleKey.Escape)
                {
                    IsActive = false;
                    CurrentExitStatus = MenuExitStatus.Exit;
                    Current = null;
                    Window.CurrentScreen.Cursor.Coord = Coord;
                }
                else if (key == ConsoleKey.Enter || key == pressKey)
                {
                    if (Type == MenuType.VerticalInput && !Current.IsController)
                    {
                        inputSelected = true;
                        continue;
                    }
                    IsActive = false;
                    CurrentExitStatus = MenuExitStatus.Press;
                    if (ClearAfter)
                        Clear();
                    Window.CurrentScreen.Cursor.Coord = Coord;
                    if (Current.SubMenu.Objects.Count > 0)
                    {
                        Current.SubMenu.IsActive = true;
                        Current.SubMenu.LoopStarting += delegate
                                                        {
                                                            Clear();
                                                            Print();
                                                        };
                        Current.SubMenu.Run();
                        if (Current.SubMenu.CurrentExitStatus == MenuExitStatus.Back)
                        {
                            Clear();
                            Print();
                            IsActive = true;
                        }
                    }
                    Selected?.Invoke(this, new SelectedEventArgs<MenuItem>(Current, Current.Index));
                }
                else if (key == finishKey && IsSubMenu)
                {
                    IsActive = false;
                    CurrentExitStatus = MenuExitStatus.Back;
                }
                else if (key == nextKey)
                    SelectNext();
                else if (key == prevKey)
                    SelectPrevious();
            }
        }
    }
}