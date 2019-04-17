using System;

namespace Pentagon.Utilities.Console.Demo
{
    using System.Globalization;
    using Extensions.Console.Controls;
    using Extensions.Security;
    using Console = System.Console;

    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("cs");

            var swi = new SwitchCliControl("Is itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs it?", true);

          var result =  swi.Run();

            Console.WriteLine();

            var sad = new SecretTextCliControl("Name id",  SecretTextOutputMode.PeekLast).Run();

            Console.WriteLine();
            Console.WriteLine(sad.ConvertToString());
            Console.ReadKey();
        }
    }
}
