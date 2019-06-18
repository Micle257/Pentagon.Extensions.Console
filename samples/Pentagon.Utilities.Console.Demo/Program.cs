using System;

namespace Pentagon.Utilities.Console.Demo
{
    using System.Globalization;
    using ConsolePresentation.Controls.Inputs;
    using Extensions.Console;
    using Extensions.Console.Controls;
    using Extensions.Security;
    using Console = System.Console;

    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("cs");

            var combo = new ComboBoxCliControl<string>("Teststets asd asd asd asd sad asd sad sad sad gfgh fg fg fg fg fg fg fg fg f gfg f gfg fg f gf?", new []{ "LOl", "no", "piss" , "Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd Very asd ", "no" });

            var comboFields = combo.Run();
        }
    }
}
