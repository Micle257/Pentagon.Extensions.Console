// -----------------------------------------------------------------------
//  <copyright file="MenuItem.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Controls.Menu
{
    using System;
    using System.Linq;
    using Buffers;
    using Collections;
    using Inputs;
    using Structures;

    /// <summary> Class representing a item in <see cref="Menu" /> object. </summary>
    public class MenuItem : IContainable<Menu>
    {
        internal Text selectChar;

        bool inic;
        string m_default = "";

        /// <summary> Initializes a new instance of the <see cref="MenuItem" /> class with <see cref="MenuType.VerticalInput" /> type. </summary>
        /// <param name="menu"> The menu container. </param>
        /// <param name="name"> The name. </param>
        /// <param name="intp"> The general <see cref="InputType" /> of sub menu items. </param>
        public MenuItem(Menu menu, string name, InputType intp, MenuType subMenu)
        {
            Owner = menu;
            ColorName = ConsoleColour.Text;
            Owner.Objects.Add(this);
          //  ColorNameDisabled = ConsoleColours.Gray;
          //  ColorNameSelected = ConsoleColours.HText;
            NameText = new Text(name, ColorName, new BufferPoint(Owner.Coord.X + 1, Owner.Coord.Y), false);
          //  ValueText = new Text(input: "", color: ConsoleColours.Gray, coord: new BufferPoint(Owner.Coord.X + NameText.Data.Length + 2, Owner.Coord.Y + 1), moveCursor: false);
            DefaultValue = "";
            IsDisabled = false;
            HasCustomPos = false;
            Input = new Input(intp, charLimit: 64);
            Input.AllowMovement = false;
            Owner.Selected += (s, w) =>
                              {
                                  if (w.Index == Index)
                                      Pressed?.Invoke(this, null);
                              };

          //  selectChar = new Text(Owner.SelectingChar, ConsoleColours.Text, NameText.X - 1, NameText.Y);
            SubMenu = new Menu(subMenu, Owner.Coord.X + 1, Owner.Coord.Y + Index + 1);
            SubMenu.IsActive = false;
            SubMenu.IsSubMenu = true;

            if (Owner.Current == null)
                Owner.Select(this);
        }

        public MenuItem(Menu menu, string name, InputType intp) : this(menu, name, intp, MenuType.Vertical) { }

        public MenuItem(Menu menu, string name) : this(menu, name, InputType.String, MenuType.Vertical) { }

        internal MenuItem(Menu menu, string name, BufferPoint coord) : this(menu, name)
        {
            if (menu.Type != MenuType.Horizontal)
                throw new ArgumentException(message: "Custom coord is available only in horizontal menu.");
            HasCustomPos = true;
            CustomPos = coord;
        }

        /// <summary> Occurs when this item is selected. </summary>
        public event EventHandler Pointing;

        public event EventHandler Pointed;

        /// <summary> Occurs when this item is pressed. </summary>
        public event EventHandler Pressed;

        /// <summary> Gets the name. </summary>
        public string Name => NameText.Data;

        /// <summary> Gets the string lenght of <see cref="Name" /> and <see cref="Value" /> combined. </summary>
        public int Lenght => (Value?.Length ?? -1) + NameText.Data.Length + 1;

        public BufferPoint Coord => NameText.Coord;

        /// <summary> Gets or sets the value (description). </summary>
        public string Value
        {
            get => ValueText.Data;
            set
            {
                if (Owner.Type != MenuType.Horizontal)
                    ValueText.Data = value;
            }
        }

        public string DefaultValue { get; set; }

        /// <summary> Gets or sets a value indicating whether this item is disabled. </summary>
        public bool IsDisabled { get; set; }

        /// <summary> Gets a value indicating whether this instance is selected (pointed). </summary>
        public bool IsSelected { get; private set; }

        public Menu SubMenu { get; internal set; }
        public ConsoleColour ColorNameSelected { get; internal set; }
        public ConsoleColour ColorName { get; internal set; }
        public ConsoleColour ColorNameDisabled { get; internal set; }

        public InputType InputType
        {
            get => Input.Type;
            set => Input = new Input(value, charLimit: 64);
        }

        public bool IsController { get; internal set; }
        public int Index => Owner?[this] ?? -1;

        public Menu Owner { get; }

        internal Text ValueText { get; }
        internal Text NameText { get; }
        internal bool HasCustomPos { get; }
        internal BufferPoint CustomPos { get; }
        internal Input Input { get; set; }

        /// <summary> Adds the sub menu item. </summary>
        /// <param name="str"> The name of submenu item. </param>
        /// <exception cref="System.ArgumentException"> Non vertical menu cannot have a menu item with sub menu. </exception>
        /// <exception cref="System.NotSupportedException"> Cannot create a second level of sub menu. </exception>
        public MenuItem AddSubMenuItem(string str, InputType input)
        {
            if (Owner.Type != MenuType.Vertical)
                throw new ArgumentException(message: "Non vertical menu can't have a sub menu.");
            if (Owner.IsSubMenu)
                throw new NotSupportedException(message: "Cannot create a second level of sub menu.");

            return SubMenu.Add(str, input);
        }

        public MenuItem AddSubMenuItem(string str) => AddSubMenuItem(str, Input.Type);

        /// <summary> Selects this instance. </summary>
        public void Select()
        {
            IsSelected = true;
            Pointing?.Invoke(this, null);
        }

        internal void TextPrint()
        {
            if (inic && selectChar.Data != " ")
                selectChar.Clear();
            Initialize();
            if (IsSelected && selectChar.Data != " ")
                selectChar.Print();
            NameText.Print();
            //if (Owner.Type == MenuType.Vertical)
            ValueText?.Print();
        }

        internal void TextClear()
        {
            if (selectChar.Data != " ")
                selectChar.Clear();
            NameText.Clear();
            if (ValueText.Data != null)
                ValueText.Clear();
        }

        internal void UnSelect()
        {
            IsSelected = false;
            Pointed?.Invoke(this, null);
        }

        void Initialize()
        {
            switch (Owner.Type)
            {
                case MenuType.VerticalInput:
                case MenuType.Vertical:
                    NameText.Y = Index
                                 + Owner.Coord.Y
                                 + Owner.Objects.Where(a => a.Index < Index && a.SubMenu.IsActive).Select(a => a.SubMenu.Objects.Count).Sum();
                    ValueText.Y = NameText.Y;
                    break;
                case MenuType.Horizontal:
                    NameText.X = Owner.Coord.X;
                    NameText.X += Owner.Objects.Count(a => a.Index < Index) * (Owner.SelectingChar != ' ' ? 2 : 1);
                    NameText.X += Owner.Objects.Where(a => a.Index < Index)?.Select(a => a.Lenght).Sum() ?? 0;
                    if (HasCustomPos)
                        NameText.X = CustomPos.X;
                    ValueText.Data = null;
                    break;
            }

            // Select char
            if (Owner.inputSelected)
            {
                selectChar.X = ValueText.X - 1;
                selectChar.Y = NameText.Y;
            }
            else
            {
                selectChar.X = NameText.X - 1;
                selectChar.Y = NameText.Y;
            }

            // Colors
            if (IsSelected)
                NameText.Color = ColorNameSelected;
            else
                NameText.Color = ColorName;
            if (IsDisabled)
                NameText.Color = ColorNameDisabled;
            if (Owner.inputSelected)
            {
               // if (IsSelected)
                //    ValueText.Color = ConsoleColours.White;
               // else
               //     ValueText.Color = ConsoleColours.Gray;
            }
           // else
           //     ValueText.Color = ConsoleColours.Gray;

            Input.Coord = new BufferPoint(ValueText.Coord.X, ValueText.Coord.Y);
            if (IsSelected)
            {
                if (Owner.inputSelected)
                {
                    ConsoleWindow.CurrentWindow.CurrentScreen.Cursor.Coord = new BufferPoint(Input.Coord.X + Input.InputText.Length, Input.Coord.Y);
                    ConsoleWindow.CurrentWindow.CurrentScreen.Cursor.Show = Input.ShowCursor;
                    ConsoleWindow.CurrentWindow.CurrentScreen.Cursor.Size = Input.CursorSize;
                }
                else
                    ConsoleWindow.CurrentWindow.CurrentScreen.Cursor.Show = false;
            }
            inic = true;
        }
    }
}