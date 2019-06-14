using System;

namespace Pentagon.Utilities.Console.Demo
{
    using System.Globalization;
    using Extensions.Console;
    using Extensions.Console.Controls;
    using Extensions.Security;
    using Console = System.Console;

    class Program
    {
        static void Main(string[] args)
        {
            goto lo;

            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("cs");

            var swi = new SwitchCliControl("Is it?", true);

            var result =  swi.Run();

            Console.WriteLine();
            Console.Write("SAS");

            var lol = new TextCliControl
                      {
                              DefaultValue = "non",
                              TypedText = "pol",
                              Text = "D ...d",
                              Label = CliLabels.Warning
                      };

            var results = lol.Run();

            Console.WriteLine();

            var sad = new SecretTextCliControl("Name id",  SecretTextOutputMode.PeekLast).Run();

            Console.WriteLine();
            Console.WriteLine(sad.ConvertToString());

            lo:
            var cursor = Cursor.FromCurrentPosition(new SystemConsole());
            cursor.Show = true;

            while (true)
            {
                cursor.Apply();
                cursor.EnsureNewLine();

                Console.Read();

                ConsoleHelper.WriteSuccess("EJ");
                Console.WriteLine();

                cursor.Offset(0, 5);
                cursor.X = 1;
            }
        }
    }
}
