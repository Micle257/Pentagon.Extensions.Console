using System;

namespace Pentagon.Utilities.Console.Demo
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ConsolePresentation;
    using ConsolePresentation.Buffers;
    using ConsolePresentation.ColorSystem;
    using ConsolePresentation.Controls;
    using ConsolePresentation.Controls.Borders;
    using ConsolePresentation.Enums;
    using ConsolePresentation.Structures;
    using Console = System.Console;

    class Program
    {
        static ConsoleWindow consoleWindow;

        static void Main(string[] args)
        {
            var colorProvider = new ConsoleColorProvider().WithBlankColor(new ConsoleColour(DefaultColorScheme.Black, DefaultColorScheme.Black)).WithTextColor(new ConsoleColour(DefaultColorScheme.White, DefaultColorScheme.Black));
            var screenProvider = new ScreenFactory(new ConsoleWriter(new BufferFilter(colorProvider), colorProvider), new ConsoleCleaner(new CleanerFilter(colorProvider), colorProvider), new ColorManager(new ColorMapper(), new ColorSchemeManager()), new ScreenCellCache());
            var screen = screenProvider.Create<DefaultColorScheme>();
            screen.Width = 100;
            screen.Height = 70;
            consoleWindow = new ConsoleWindow(screenProvider);
            consoleWindow.Width = 100;
            consoleWindow.Height = 70;
            consoleWindow.Sizeable = false;
            consoleWindow.AddScreen(screen);
            consoleWindow.SelectScreen(screen);
            screen.WithPanel(new StackPanel());
            screen.ApplyColorTheme();
            consoleWindow.CreateConsole();

            var s = new CancellationTokenSource();
            var task = consoleWindow.RunAsync(s.Token);
            
            var runTask = Task.Run((Action)TextBlock).ContinueWith(t =>
                                                                   {
                                                                       if (t.IsFaulted)
                                                                       {
                                                                           InputListener.Current.ErrorStop(t.Exception);
                                                                           s.Cancel();
                                                                       }
                                                                   });

            Task.WaitAll(task, runTask);
        }
        
        static void TextBlock()
        {
            var sda = new TextBlock();
            sda.Border = new Border(new BorderLine(BorderLineType.DoubleLine));
            sda.Data = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis sollicitudin nunc nec sem blandit, quis tincidunt est convallis. Integer et lectus molestie erat aliquet commodo. Vivamus feugiat nisl vitae porta fringilla. Integer tempus, ante id vehicula blandit, massa nunc lacinia tellus, id sodales ex eros quis turpis. Cras et orci.";
            sda.Padding = new Thickness(0);
            sda.Margin = new Thickness(3);
            sda.Color = new ConsoleColour(DefaultColorScheme.Cyan, DefaultColorScheme.Black);
            sda.TextWrapping = TextWrapping.Wrap;
            sda.Size = 30;
            sda.TextAlignment = TextAlignment.Right;
            consoleWindow.CurrentScreen.DrawControl(sda);

            var t = new InputBox();
            t.Border = new Border(new BorderLine(BorderLineType.Line));
            t.Margin = new Thickness(2);
            t.HasPrefix = true;
            t.ShowOverflowPrefixCharacter = true;
            t.PrefixCharacterColor = new ConsoleColour(DefaultColorScheme.Gray);
            t.AppendText("HII");
            t.Padding = new Thickness(1);
            consoleWindow.CurrentScreen.DrawControl(t);
            
            //var tb = new TextBlock
            //        {
            //            Border = new Border(new BorderLine(BorderLineType.Line)),
            //            Data = "LOL",
            //            Color = ConsoleColours.Blue,
            //            Margin = new Thickness(1),
            //            Size = 7
            //        };
            //consoleWindow.CurrentScreen.DrawControl(tb);

            consoleWindow.CurrentScreen.FocusControl(t);
        }
    }
}
