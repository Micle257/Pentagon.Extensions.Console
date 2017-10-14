// -----------------------------------------------------------------------
//  <copyright file="TimeInput.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Controls.Inputs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Menu;
    using Pentagon.Extensions;
    using Structures;

    public class TimeInput
    {
        internal List<MenuItem> Items;
        internal List<string> TimeData;
        readonly Text m_error;

        public TimeInput(TimeFormatType FormatType, BufferPoint coord)
        {
            Coord = coord;
            this.FormatType = FormatType;
            Menu = new Menu(MenuType.Horizontal, Coord.X, Coord.Y);
         //   m_error = new Text(data: "Invalid input.", color: ConsoleColours.Red, x: 1, y: Coord.Y);
            m_error.MoveCursor = false;
            Items = new List<MenuItem>();
            TimeData = new List<string>();
            Grid = new Text(input: "    ", color: ConsoleColour.Text, coord: Coord);
            switch (FormatType)
            {
                case TimeFormatType.Millisecond:
                case TimeFormatType.Second:
                case TimeFormatType.Minute:
                case TimeFormatType.Hour:
                case TimeFormatType.Day:
                    Items.Add(new MenuItem(Menu, name: "dd", coord: Coord));
                    Items.Add(new MenuItem(Menu, name: "MM", coord: new BufferPoint(Coord.X + 3, Coord.Y)));
                    Items.Add(new MenuItem(Menu, name: "yyyy", coord: new BufferPoint(Coord.X + 6, Coord.Y)));
                    Grid.Data = "  /  /    ";
                    break;
                case TimeFormatType.Month:
                    Items.Add(new MenuItem(Menu, name: "MM", coord: Coord));
                    Items.Add(new MenuItem(Menu, name: "yyyy", coord: new BufferPoint(Coord.X + 3, Coord.Y)));
                    Grid.Data = "  /    ";
                    break;
                case TimeFormatType.Year:
                    Items.Add(new MenuItem(Menu, name: "yyyy", coord: new BufferPoint(Coord.X, Coord.Y)));
                    break;
            }
            Items.Reverse();
            if ((int) FormatType > (int) TimeFormatType.Day)
            {
                Items.Add(new MenuItem(Menu, name: "HH", coord: new BufferPoint(Coord.X + 11, Coord.Y)));
                Grid.Data += "   ";
                if ((int) FormatType > (int) TimeFormatType.Hour)
                {
                    Items.Add(new MenuItem(Menu, name: "mm", coord: new BufferPoint(Coord.X + 14, Coord.Y)));
                    Grid.Data += ":  ";
                    if ((int) FormatType > (int) TimeFormatType.Minute)
                    {
                        Items.Add(new MenuItem(Menu, name: "ss", coord: new BufferPoint(Coord.X + 17, Coord.Y)));
                        Grid.Data += ":  ";
                        if ((int) FormatType > (int) TimeFormatType.Second)
                        {
                            Items.Add(new MenuItem(Menu, name: "fff", coord: new BufferPoint(Coord.X + 20, Coord.Y)));
                            Grid.Data += ".   ";
                        }
                    }
                }
            }
            var con = Menu.AddController(str: "Confirm");
            con.Pointing += (s, e) =>
                            {
                             //   foreach (var item in Menu.Objects)
                              //      item.ColorName = ConsoleColours.Gray;
                            };
            con.Pointed += delegate
                           {
                            //   foreach (var item in Menu.Objects)
                              //     item.ColorName = ConsoleColours.White;
                           };
            con.Pressed += (s, e) =>
                           {
                               foreach (var item in Items.Where(a => !a.IsController))
                               {
                                   var num = 0;
                                  // var m_error = new Text(input: "", color: ConsoleColours.Red, coord: new BufferPoint(Coord.X + 10, Coord.Y));
                                   try
                                   {
                                       if (!int.TryParse(item.Name, out num))
                                       {
                                           m_error.Data = "All fiels must be integers.";
                                           throw new Exception();
                                       }
                                       m_error.Data = "Time field must be in range.";
                                       //Time = Items.Select(a => a.Name.ToInt()).ToTime(Type); // TODO make time
                                   }
                                   catch
                                   {
                                       m_error.X = ((MenuItem) s).Coord.X + 10;
                                       m_error.Print();
                                       Menu.Select(item);
                                       Grid.Print();
                                       Menu.IsActive = true;
                                   }
                               }
                               //Time = Items.Select(a => a.Name.ToInt()).ToTime(Type); // TODO also
                           };
            Menu.Selected += (s, ar) =>
                             {
                                 var e = ar.SelectedItem;
                                 if (e.IsController)
                                     return;
                                 var origLen = e.Name.Length;
                                 var input = new Input(InputType.Int, charLimit: origLen);
                                 input.Coord = e.Coord;
                                 input.InputColor = e.ColorNameSelected;
                                 input.Run();
                                 e.NameText.Data = input.InputText.ToInt().ToFillString(origLen);
                                 Menu.IsActive = true;
                                 Menu.SelectNext();
                             };
        }

        public BufferPoint Coord { get; }
        public TimeFormatType FormatType { get; }
        public DateTime Time { get; private set; }
        internal Menu Menu { get; }
        internal Text Grid { get; }

        public void Run()
        {
            Grid.Print();
            Menu.Run();
        }
    }
}